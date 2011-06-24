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

namespace Resolver
{
    public interface IRemote : IPeerServices, IClientChannel
    { 
    
    }

    public class Resolver : Peer
    {
        #region CONSTs HERE
        /********************************/
        private const int TTL = 2;
        /********************************/
        #endregion

        #region Attributes

        private string _originPeerName;

        private CustomResolver crs = new CustomResolver { ControlShape = false };
        private ServiceHost customResolver;
        
        private ServiceHost remoteHost;
        //private IPeerServices remoteChannel;
        private IRemote remoteChannel;
        private List<RemoteConnection> _incomingConnections = new List<RemoteConnection>();

        private ObservableCollectionEx<TempBuilding> _buildings = new ObservableCollectionEx<TempBuilding>();
        private MessageHandler MsgHandler;
        private PeerServices remoteMessageHandler;

        public string name { get; private set; }
        
        private PeerStatus _peerStatus;
        private bool isLocalConnected;
        private bool isRemoteConnected;
        private List<EnergyProposalMessage> _proposalList = new List<EnergyProposalMessage>();        

        private List<TempBuilding> hbrBuildingd = new List<TempBuilding>();

        private System.Timers.Timer _HBTimer;

        private object _lLock = new object();

        private EnergyBroker _broker;

        #endregion

        #region Methods
        
        public Resolver() : base(Tools.getResolverName(),PeerStatus.Resolver)
        {
            this.name = Tools.getResolverName();            

            this.isLocalConnected = false;
            this.isRemoteConnected = false;
            
            this._peerStatus = PeerStatus.Resolver;

            //This timer manage the peer's HB to check the online status
            _HBTimer = new System.Timers.Timer();
            _HBTimer.Interval = 5000;
            _HBTimer.Elapsed += new ElapsedEventHandler(_HBTimer_Elapsed);
            _HBTimer.Enabled = false;
        }

        public void Connect()
        {
            this.isLocalConnected = StartLocalResolver();
            this.isRemoteConnected = StartRemoteConnection();            
              
            _HBTimer.Enabled = true;            

            #region Normal Peer Activity

            base.StartService();
            MsgHandler = Connector.messageHandler;

            _broker = new EnergyBroker(name);

            #region Event Listeners
            MsgHandler.OnForwardEnergyRequest += new forwardEnergyRequest(ForwardEnergyRequest);
            MsgHandler.OnForwardEnergyReply += new forwardEnergyReply(ForwardEnergyReply);
            MsgHandler.OnSayHello += new sayHello(HelloResponse);
            MsgHandler.OnHeartBeat += new heartBeat(CheckHeartBeat);
            MsgHandler.OnUpdateStatus += new updateStatus(UpdatePeerStatus);
            #endregion

            #endregion
        }

        private bool StartLocalResolver()
        {
            bool bRet = false;

            customResolver = new ServiceHost(crs);
            
            XMLLogger.WriteLocalActivity("Starting Custom Local Peer Resolver Service...");

            try
            {
                crs.Open();
                customResolver.Open();
                bRet = true;
                XMLLogger.WriteLocalActivity("Custom Local Peer Resolver Service is started");                
            }
            catch (Exception e)
            {
                XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), "Error in starting Custom Local Peer Resolver Service");
                XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), e.ToString()); 
                crs.Close();
                customResolver.Abort();
                bRet = false;
            }

            return bRet;
        }

        private bool StartRemoteConnection()
        {            
            int n=0;
            bool bRet = false;
            List<RemoteHost> h;            
            
            remoteMessageHandler = new PeerServices();

            remoteHost = new ServiceHost(remoteMessageHandler);

            //To handle the remote traffic            
            remoteMessageHandler.OnRemoteEnergyRequest += new manageEnergyRequest(ManageRemoteEnergyRequest);
            remoteMessageHandler.OnRemoteEnergyReply += new replyEnergyRequest(ManageRemoteEnergyReply);
            remoteMessageHandler.OnRemotePeerIsDown += new remotePeerIsDown(RemotePeerIsDown);

            h = Tools.getRemoteHosts();
            
            while (bRet == false && n < h.Count)
            {
                //To connect to remote host
                NetTcpBinding tcpBinding = new NetTcpBinding();
                EndpointAddress remoteEndpoint = new EndpointAddress(h[n].netAddress);
                tcpBinding.Security.Mode = SecurityMode.None;                

                //ChannelFactory<IPeerServices> cf = new ChannelFactory<IPeerServices>(tcpBinding, remoteEndpoint);
                ChannelFactory<IRemote> cf = new ChannelFactory<IRemote>(tcpBinding, remoteEndpoint);
                remoteChannel = cf.CreateChannel();

                try
                {
                    remoteHost.Open();

                    XMLLogger.WriteRemoteActivity("Remote service started.");
                    XMLLogger.WriteRemoteActivity("Connecting to " + h[n].IP);

                    //Retrieve Remote IP Addresses
                    foreach (var newRemote in remoteChannel.RetrieveContactList())
                    {
                        if (!h.Exists(delegate(RemoteHost x) { return x.netAddress == newRemote.netAddress; }))
                        {
                            h.Add(newRemote);
                            Tools.updateRemoteHosts(newRemote);
                        }
                    }

                    XMLLogger.WriteRemoteActivity("Connected to: " + h[n].IP);                    
                    bRet = true;
                }
                catch (Exception e)
                {
                    XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), "Error in connecting to: " + h[n].IP);
                    XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), e.ToString()); //For debug purpose   
                    n++;
                    if(n > h.Count)
                        remoteHost.Abort();

                    bRet = false;
                }
            }

            return bRet;
        }

        private void ForwardEnergyRequest(StatusNotifyMessage message)   
        {
            if (isRemoteConnected == true)
            {
                XMLLogger.WriteRemoteActivity("Forwarding Energy Request from: " + message.header.Sender );

                remoteChannel.ManageRemoteEnergyRequest(MessageFactory.createRemoteEnergyRequestMessage(message,
                    this.name,
                    Tools.getLocalIP(),
                    Tools.getResolverServicePort()
                    ));
            }
            else
                XMLLogger.WriteRemoteActivity("No Remote Connections. Please Check your NetConfig file.");
        }

        private void ManageRemoteEnergyRequest(RemoteEnergyRequest message)
        {
            RemoteConnection remConn;

            XMLLogger.WriteRemoteActivity("Received Remote Energy Request from: " + message.enReqMessage.header.Sender + " by Remote Resolver: " + message.header.Sender);
            XMLLogger.WriteRemoteActivity("Message ID: " + message.enReqMessage.header.MessageID);

            remConn = GetConnection(message.IP, message.port);

            if (remConn == null)//If entry doesn't exist
            {
                string address = @"net.tcp://" + message.IP + ":" + message.port + @"/Remote";

                NetTcpBinding tcpBinding = new NetTcpBinding();
                EndpointAddress remoteEndpoint = new EndpointAddress(address);
                tcpBinding.Security.Mode = SecurityMode.None;

                ChannelFactory<IRemote> cf = new ChannelFactory<IRemote>(tcpBinding, remoteEndpoint);
                IRemote tChannel = cf.CreateChannel();

                remConn = new RemoteConnection()
                {
                    channel = tChannel,
                    IP = message.IP,
                    port = message.port
                };
            }

            remConn.requests.Add(message.enReqMessage.header.MessageID, message.enReqMessage.header.Sender);

            _incomingConnections.Add(remConn);

            //Header handling
            message.enReqMessage.header.Sender = this.name;
            
            _broker.EnergyLookUp(message.enReqMessage);
        }

        void ForwardEnergyReply(EndProposalMessage message)
        {            
            RemoteConnection conn = GetConnectionByMessageID(message.header.MessageID);

            if (conn != null)
            {
                XMLLogger.WriteRemoteActivity("Forwarding Remote Response about message: " + message.header.MessageID + " Status = " + message.endStatus);

                //Header re-handling
                message.header.Receiver = conn.requests[message.header.MessageID];
                conn.channel.ReplyEnergyRequest(message);

                RemoveRequestEntry(message.header.MessageID);
            }
            else
            {
                XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), "Could not find the following message: " + message.header.MessageID);
            }
        }

        void ManageRemoteEnergyReply(EndProposalMessage message)
        {
            XMLLogger.WriteRemoteActivity("Received Remote Energy Reply from:" + message.header.Sender);

            Connector.channel.endProposal(message);
        }

        private void RemotePeerIsDown(PeerIsDownMessage message)
        {
            Connector.channel.peerDown(message);
        }

        private void HelloResponse(HelloMessage message)
        {
            //Elaborate the Hello Message
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
                        
                        // TODO: fix here                        
                        //foreach (var c in _incomingConnections)
                        //    c.channel.PeerDownAlert(MessageFactory.createPeerIsDownMessage("@All", this.name, _buildings[i].Name));

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

        public void CloseService()
        {
            XMLLogger.WriteLocalActivity("Closing Application...");

            _HBTimer.Enabled = false;

            if (this.isRemoteConnected == true)                        
                remoteHost.Close();
            
            if(this.isLocalConnected == true)
            {                
                crs.Close();
                customResolver.Close();                
            }

            StopService(); //Calls the base.StopService method
        }

        private RemoteConnection GetConnection(string IP, string port)
        {
            RemoteConnection cRet = null;

            foreach (var c in _incomingConnections)
            {
                if (c.IP == IP && c.port == port)
                {
                    cRet = c;
                    break;
                }
            }

            return cRet;
        }

        private RemoteConnection GetConnectionByMessageID(Guid ID)
        {
            foreach (var c in _incomingConnections)
            {
                if(c.requests.ContainsKey(ID))                
                    return c;
            }

            return null;
        }

        private void RemoveRequestEntry(Guid ID)
        {           
            for(int i=0;i<_incomingConnections.Count;i++)
            {
                if (_incomingConnections[i].requests.ContainsKey(ID))
                    _incomingConnections[i].requests.Remove(ID);

                if (_incomingConnections[i].requests.Count == 0)
                {
                    _incomingConnections[i].channel.Close();
                    _incomingConnections.RemoveAt(i);
                }
            }
        }

        #endregion

        private class RemoteConnection
        {
            public IRemote channel;            
            public Dictionary<Guid, string> requests = new Dictionary<Guid,string>();
            public string IP;
            public string port;
        }    
    }
}
