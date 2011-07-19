using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using SmartGridManager.Core;
using SmartGridManager.Core.Commons;
using SmartGridManager.Core.Utils;
using SmartGridManager.Core.P2P;
using SmartGridManager.Core.Messaging;
using System.Timers;
using System.Collections.ObjectModel;
using System.Windows;
using System.ServiceModel.PeerResolvers;

namespace Resolver
{
    public class Resolver : Peer
    {
        #region CONSTs HERE
        /********************************/
        private const int TTL = 2;
        /********************************/
        #endregion

        #region Attributes

        private CustomPeerResolverService _crs = new CustomPeerResolverService();
        private ServiceHost _customResolver;
        private MessageHandler _msgHandler;

        private ServiceHost _remoteHost;        
        private IRemote _remoteChannel;        
        private PeerServices _remoteMessageHandler;        

        private List<RemoteHost> _remoteResolvers = new List<RemoteHost>();
        private List<RemoteConnection> _remoteConnections = new List<RemoteConnection>();                

        private ObservableCollectionEx<TempBuilding> _buildings = new ObservableCollectionEx<TempBuilding>();

        public string name { get; private set; }

        private int _nHostIndex = 0;
        private System.Timers.Timer _HBTimer;

        private Thread _brokerThread;
        private Thread _requestThread;
        
        public bool isLocalConnected { get; private set; }
        public bool isRemoteServiceStarted { get; private set; }
        public bool isRemoteConnected { get; private set; }

        private object _lLock = new object();
        private object _connectionLock = new object();
        private object _counterLock = new object();

        private EnergyBroker _broker;

        #endregion

        #region Methods

        public Resolver() : base(Tools.getResolverName(),PeerStatus.Resolver)
        {
            this.name = Tools.getResolverName();            

            this.isLocalConnected = false;
            this.isRemoteServiceStarted = false;
            this.isRemoteConnected = false;            

            //This timer manage the peer's HB to check the online status
            _HBTimer = new System.Timers.Timer();
            _HBTimer.Interval = 3500;
            _HBTimer.Elapsed += new ElapsedEventHandler(_HBTimer_Elapsed);
            _HBTimer.Enabled = false;
        }

        public void Connect()
        {
            this.isLocalConnected = StartLocalResolver();
            
            if (this.isLocalConnected == true)
            {
                this.isRemoteServiceStarted = StartRemoteService();                

                _HBTimer.Enabled = true;

                #region Normal Peer Activity

                base.StartService();
                _msgHandler = Connector.messageHandler;

                _broker = new EnergyBroker(name);

                #region Event Listeners
                _msgHandler.OnForwardEnergyRequest += new forwardEnergyRequest(ForwardEnergyRequest);
                _msgHandler.OnForwardEnergyReply += new forwardEnergyReply(ForwardEnergyReply);
                _msgHandler.OnSayHello += new sayHello(HelloResponse);
                _msgHandler.OnHeartBeat += new heartBeat(CheckHeartBeat);
                _msgHandler.OnUpdateStatus += new updateStatus(UpdatePeerStatus);                
                #endregion

                #endregion
            }
        }

        #region Connection Methods Section

        private bool StartLocalResolver()
        {
            bool bRet = false;

            _customResolver = new ServiceHost(_crs);
            
            XMLLogger.WriteLocalActivity("Starting Custom Local Peer Resolver Service...");

            try
            {
                _crs.Open();
                _customResolver.Open();
                bRet = true;
                XMLLogger.WriteLocalActivity("Custom Local Peer Resolver Service is started");                
            }
            catch (Exception e)
            {
                XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), "Error in starting Custom Local Peer Resolver Service");
                XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), e.ToString()); 
                _crs.Close();
                _customResolver.Abort();
                bRet = false;
            }

            return bRet;
        }

        private bool StartRemoteService()
        {                        
            bool bRet = false;            
            
            _remoteMessageHandler = new PeerServices();

            _remoteHost = new ServiceHost(_remoteMessageHandler);

            //To handle the remote traffic            
            _remoteMessageHandler.OnRemoteEnergyRequest += new manageEnergyRequest(ManageRemoteEnergyRequest);
            _remoteMessageHandler.OnRemoteEnergyReply += new replyEnergyRequest(ManageRemoteEnergyReply);
            _remoteMessageHandler.OnRemotePeerIsDown += new remotePeerIsDown(RemotePeerIsDown);            

            try
            {
                _remoteHost.Open();
                bRet = true;
                XMLLogger.WriteRemoteActivity("Remote service started.");
            }
            catch (Exception e)
            {                
                XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), "Unable to start Remote Service.");                
                //XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), e.ToString()); //For debug purpose
                _remoteHost.Abort();
            }

            return bRet;
        }

        private void ConnectToRemoteHost(object m)
        {
            bool connected = false;
            StatusNotifyMessage message = (StatusNotifyMessage)m;
            _remoteResolvers = Tools.getRemoteHosts();

            while (connected == false && _nHostIndex < _remoteResolvers.Count)
            {
                //localhost connections filter
                if (_remoteResolvers[_nHostIndex].IP == "127.0.0.1" || _remoteResolvers[_nHostIndex].IP.ToLower() == "localhost")
                {
                    lock (_counterLock)
                    {
                        if (_nHostIndex < (_remoteResolvers.Count - 1))
                            _nHostIndex++;
                        else
                            _nHostIndex = 0;
                    }

                    continue;
                }

                XMLLogger.WriteRemoteActivity("Connecting to " + _remoteResolvers[_nHostIndex].IP);
                
                NetTcpBinding tcpBinding = new NetTcpBinding();
                EndpointAddress remoteEndpoint = new EndpointAddress(_remoteResolvers[_nHostIndex].netAddress);
                tcpBinding.Security.Mode = SecurityMode.None;
                
                ChannelFactory<IRemote> cf = new ChannelFactory<IRemote>(tcpBinding, remoteEndpoint);
                _remoteChannel = cf.CreateChannel();

                lock (_counterLock)
                {
                    try
                    {
                        _remoteChannel.Open();

                        //Retrieve Remote IP Addresses
                        foreach (var newRemote in _remoteChannel.RetrieveContactList())
                        {
                            if (!_remoteResolvers.Exists(delegate(RemoteHost x) { return x.netAddress == newRemote.netAddress; }))
                            {
                                _remoteResolvers.Add(newRemote);
                                Tools.updateRemoteHosts(newRemote);
                            }
                        }

                        XMLLogger.WriteRemoteActivity("Connected to: " + _remoteResolvers[_nHostIndex].IP);

                        XMLLogger.WriteRemoteActivity("Forwarding Energy Request from: " + message.header.Sender + "To: " + _remoteResolvers[_nHostIndex]);
                        XMLLogger.WriteRemoteActivity("Message ID: " + message.header.MessageID);

                        _remoteChannel.ManageRemoteEnergyRequest(MessageFactory.createRemoteEnergyRequestMessage(message,
                            _remoteResolvers[_nHostIndex].name,
                            this.name,
                            Tools.getLocalIP(),
                            Tools.getResolverServicePort()
                            ));

                        this.isRemoteConnected = true;
                        connected = true;
                    }

                    catch (Exception e)
                    {
                        XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), "Unable to connect to: " + _remoteResolvers[_nHostIndex].IP);
                        //XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), e.ToString()); //For debug purpose   
                        _nHostIndex++;
                        if (_nHostIndex >= _remoteResolvers.Count)
                        {
                            _nHostIndex = 0;
                            _remoteChannel.Abort();
                        }

                        connected = false;
                    }
                }
            }


            if (_nHostIndex >= _remoteResolvers.Count)
            {
                lock (_counterLock)
                {
                    _nHostIndex = 0;
                }
            }
        }
        
        #endregion

        #region Energy Messages Forwarding/Managing Section

        private void ForwardEnergyRequest(StatusNotifyMessage message)
        {
            XMLLogger.WriteRemoteActivity("Forwarding " + message.energyReq + "kW Energy Request from: " + message.header.Sender);
            XMLLogger.WriteRemoteActivity("MessageID: " + message.header.MessageID);

            _requestThread = new Thread(new ParameterizedThreadStart(ConnectToRemoteHost));
            _requestThread.Start(message);
        }

        private void ManageRemoteEnergyRequest(RemoteEnergyRequest message)
        {
            RemoteConnection remConn;

            Guid MessageID = message.enReqMessage.header.MessageID;            
            string remotePeer  = message.enReqMessage.header.Sender;

            XMLLogger.WriteRemoteActivity("Received Remote Energy Request from: " + message.enReqMessage.header.Sender + " by Remote Resolver: " + message.header.Sender);
            XMLLogger.WriteRemoteActivity("Message ID: " + message.enReqMessage.header.MessageID);

            remConn = GetConnection(message.IP, message.port, ConnectionType.Incoming);

            if (remConn == null)//If entry doesn't exist
            {
                remConn = new RemoteConnection()
                {
                    type = ConnectionType.Incoming,

                    remoteResolver = new RemoteHost()
                    {
                        name = message.header.Sender,
                        IP = message.IP,
                        port = message.port,
                        netAddress = @"net.tcp://" + message.IP + ":" + message.port + @"/Remote"
                    }
                };

                remConn.requests.Add(MessageID, new RemoteRequest(){
                    localePeerName = "", 
                    remotePeerName = remotePeer,
                    energy = 0});

               _remoteConnections.Add(remConn);
            }
            else
                remConn.requests.Add(MessageID, new RemoteRequest()
                {
                    localePeerName = "",
                    remotePeerName = remotePeer,
                    energy = 0
                });

            _brokerThread = new Thread(new ParameterizedThreadStart(_broker.EnergyLookUp));
            _brokerThread.Start(message.enReqMessage);
        }

        void ForwardEnergyReply(EndProposalMessage message)
        {                        
            RemoteConnection conn = GetConnectionByMessageID(message.header.MessageID,ConnectionType.Incoming);

            XMLLogger.WriteRemoteActivity("Forwarding Remote Response about message: " + message.header.MessageID + " Status = " + message.endStatus);
            XMLLogger.WriteRemoteActivity("Message ID: " + message.header.MessageID);

            if (conn != null)
            {                
                //Header re-handling
                conn.requests[message.header.MessageID].localePeerName = message.header.Sender;
                conn.requests[message.header.MessageID].energy = message.energy;
                message.header.Receiver = conn.requests[message.header.MessageID].remotePeerName;

                #region Creating Channel
                NetTcpBinding tcpBinding = new NetTcpBinding();
                EndpointAddress remoteEndpoint = new EndpointAddress(conn.remoteResolver.netAddress);
                tcpBinding.Security.Mode = SecurityMode.None;

                ChannelFactory<IRemote> cf = new ChannelFactory<IRemote>(tcpBinding, remoteEndpoint);
                IRemote tChannel = cf.CreateChannel();
                #endregion

                RemoteEndProposalMessage remoteEndMessage = (MessageFactory.createRemoteEndProposalMessage(message, conn.remoteResolver.name, this.name, Tools.getLocalIP(), Tools.getResolverServicePort()));

                try
                {
                    tChannel.ReplyEnergyRequest(remoteEndMessage);
                }
                catch(Exception e)
                {
                    XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), "Error in Forwarding Energy Reply Message: " + e.ToString());
                }
            }
            else
            {
                XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), "Could not find the following message: " + message.header.MessageID);
            }
        }

        void ManageRemoteEnergyReply(RemoteEndProposalMessage message)
        {            
            string localBuilding = message.endProposalMessage.header.Receiver;
            string remoteBuilding  = message.endProposalMessage.header.Sender;
            float energyBought = message.endProposalMessage.energy;

            if (message.endProposalMessage.endStatus == true)
            {
                RemoteConnection oC;

                oC = GetConnection(message.IP, message.port, ConnectionType.Outgoing);

                if (oC == null)
                {
                    oC = new RemoteConnection()
                    {
                        type = ConnectionType.Outgoing,

                        remoteResolver = new RemoteHost()
                        {
                            name = message.header.Sender,
                            IP = message.IP,
                            port = message.port,
                            netAddress = @"net.tcp://" + message.IP + ":" + message.port + @"/Remote"
                        }
                    };

                    oC.requests.Add(message.endProposalMessage.header.MessageID, new RemoteRequest()
                    {
                        localePeerName = localBuilding,
                        remotePeerName = remoteBuilding,
                        energy = energyBought
                    });

                    _remoteConnections.Add(oC);
                }
                else
                {
                    oC.requests.Add(message.endProposalMessage.header.MessageID, new RemoteRequest()
                    {
                        localePeerName = localBuilding,
                        remotePeerName = remoteBuilding,
                        energy = energyBought
                    });
                }

                XMLLogger.WriteRemoteActivity("Received Remote Energy Reply from: " + remoteBuilding + "@" + message.header.Sender);
                XMLLogger.WriteRemoteActivity("Message ID: " + message.endProposalMessage.header.MessageID);

                Connector.channel.endProposal(message.endProposalMessage);
            }
            else //No Energy From this remote resolver..Go with the next
            {
                Connector.channel.endProposal(message.endProposalMessage);

                lock (_counterLock)
                {
                    _nHostIndex++;
                }
            }
        }
        
        #endregion

        private void RemotePeerIsDown(PeerIsDownMessage message)
        {
            updateRemoteConnectionsList(message.header.Sender, message.peerName);            
            Connector.channel.peerDown(message);
        }

        private void HelloResponse(HelloMessage message)
        {
            //Elaborates the Hello Message
            TempBuilding b = new TempBuilding();

            #region setting fields
            b.Address = message.Address;
            b.Admin = message.Admin;
            b.EnBought = 0;
            b.EnSold = 0;
            b.EnPeak = message.EnPeak;
            b.EnPrice = message.EnPrice;
            b.EnProduced = message.EnProduced;            
            b.EnType = message.EnType;
            b.Name = message.header.Sender;
            b.status = message.Status;
            b.TTL = TTL;
            b.iconPath = b.status == PeerStatus.Producer ? @"/WPF_Resolver;component/img/producer.png" : @"/WPF_Resolver;component/img/consumer.png";
            #endregion

            lock(_lLock)
                _buildings.Add(b);

            XMLLogger.WriteLocalActivity("New Peer: " + b.Name + " is up!");
            
            //Be polite! Send an HelloResponse
            Connector.channel.HelloResponse(MessageFactory.createHelloResponseMessage("@All", Tools.getResolverName(), Tools.getResolverName()));
        }

        public ObservableCollectionEx<TempBuilding> GetConnectedPeers()
        {
            return _buildings;
        }

        private void CheckHeartBeat(HeartBeatMessage message)
        {
            lock (_lLock)
            {
                for (int i = 0; i < _buildings.Count; i++)
                {
                    if (_buildings[i].Name == message.header.Sender)
                        _buildings[i].TTL = TTL;
                }
            }
        }

        private void _HBTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lLock)
            {
                for (int i = 0; i < _buildings.Count; i++)
                {
                    if (_buildings[i].TTL > 0)
                        _buildings[i].TTL--;
                    else
                    {
                        XMLLogger.WriteLocalActivity("Peer: " + _buildings[i].Name + " is down!");

                        //Remove the deadly peer but first alert the folks.
                        Connector.channel.peerDown(MessageFactory.createPeerIsDownMessage("@All", this.name, _buildings[i].Name));

                        lock (_connectionLock)
                        {
                            //Alert Remote Resolvers 
                            foreach (var remConn in _remoteConnections)
                            {
                                NetTcpBinding tcpBinding = new NetTcpBinding();
                                EndpointAddress remoteEndpoint = new EndpointAddress(remConn.remoteResolver.netAddress);
                                tcpBinding.Security.Mode = SecurityMode.None;

                                ChannelFactory<IRemote> cf = new ChannelFactory<IRemote>(tcpBinding, remoteEndpoint);
                                IRemote tChannel = cf.CreateChannel();

                                try
                                {
                                    tChannel.PeerDownAlert(MessageFactory.createPeerIsDownMessage("@All", this.name, _buildings[i].Name));
                                }
                                catch (Exception ex)
                                {                                    
                                    XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), "Error in sending Remote Peer Alert Message" + ex.ToString());
                                }
                                
                            }
                        }

                        updateLocalConnectionsList(_buildings[i].Name);

                        _buildings.RemoveAt(i);                        
                    }
                }
            }            
        }

        private void UpdatePeerStatus(UpdateStatusMessage message)
        {
            if (message.header.Receiver == this.name)
            {
                TempBuilding b;

                lock (_lLock)
                {
                    for (int i = 0; i < _buildings.Count; i++)
                    {
                        if (_buildings[i].Name == message.header.Sender)
                        {
                            //Workaround for observable issue...
                            b = _buildings[i];

                            b.EnBought = message.energyBought;
                            b.EnSold = message.energySold;

                            _buildings.RemoveAt(i);
                            _buildings.Add(b);

                            break;                                        
                        }
                    }
                }
            }
        }

        public List<RemoteConnection> GetRemoteConnections()
        {
            return _remoteConnections;
        }

        public void CloseService()
        {
            XMLLogger.WriteLocalActivity("Closing Application...");

            _HBTimer.Enabled = false;

            if (this.isRemoteServiceStarted == true)                        
                _remoteHost.Close();

            if (this.isRemoteConnected == true)
                _remoteChannel.Close();

            if(this.isLocalConnected == true)
            {                
                _crs.Close();
                _customResolver.Close();                
            }

            StopService(); //Calls the base.StopService method
        }

        #region Aux Methods


        private RemoteConnection GetConnection(string IP, string port, ConnectionType type)
        {
            RemoteConnection cRet = null;

            foreach (var c in _remoteConnections)
            {
                if (c.remoteResolver.IP == IP && c.remoteResolver.port == port && c.type == type)
                {
                    cRet = c;
                    break;
                }
            }

            return cRet;        
        }

        private RemoteConnection GetConnectionByMessageID(Guid ID, ConnectionType type)
        {
            foreach (var c in _remoteConnections)
            {               
                if(c.type == type && c.requests.ContainsKey(ID))              
                    return c;
            }

            return null;
        }

        private void updateRemoteConnectionsList(string resolverName, string peerName)
        {
            lock (_connectionLock)
            {
                for (int i = 0; i < _remoteConnections.Count; i++)
                {
                    if (_remoteConnections[i].remoteResolver.name == resolverName)
                    {
                        var itemsToRemove = (from c in _remoteConnections[i].requests
                                             where c.Value.remotePeerName == peerName
                                             select c.Key).ToArray();

                        for (int j = 0; j < itemsToRemove.Length; j++)
                            _remoteConnections[i].requests.Remove(itemsToRemove[j]);

                    }
                }

                _remoteConnections.RemoveAll(delegate(RemoteConnection x) { return x.requests.Count == 0; });

            }
        }

        private void updateLocalConnectionsList(string peerName)
        {
            lock (_connectionLock)
            {
                for (int i = 0; i < _remoteConnections.Count; i++)
                {
                    var itemsToRemove = (from c in _remoteConnections[i].requests
                                         where c.Value.localePeerName == peerName
                                         select c.Key).ToArray();

                    for (int j = 0; j < itemsToRemove.Length; j++)
                        _remoteConnections[i].requests.Remove(itemsToRemove[j]);
                }

                _remoteConnections.RemoveAll(delegate(RemoteConnection x) { return x.requests.Count == 0; });
                                
            }
        }

        #endregion

        #endregion

    }
}
