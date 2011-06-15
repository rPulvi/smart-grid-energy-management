﻿using System;
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
        private CustomResolver crs = new CustomResolver { ControlShape = false };
        private ServiceHost customResolver;
        
        private ServiceHost remoteHost;
        //private IPeerServices remoteChannel;
        private IRemote remoteChannel;

        private ObservableCollectionEx<TempBuilding> _buildings = new ObservableCollectionEx<TempBuilding>();
        private MessageHandler MsgHandler;
        private PeerServices remoteMessageHandler;

        public string name { get; private set; }
        
        private PeerStatus _peerStatus;
        private bool isLocalConnected;
        private bool isRemoteConnected;
        private List<EnergyProposalMessage> _proposalList = new List<EnergyProposalMessage>();
        private List<TransactionField> _messageList = new List<TransactionField>();

        private List<TempBuilding> hbrBuildingd = new List<TempBuilding>();

        private System.Timers.Timer _HBTimer;

        private object _lLock = new object();
        #endregion

        #region Methods
        
        public Resolver() : base(Tools.getResolverName(),PeerStatus.Resolver){
            
            this.isLocalConnected = false;
            this.isRemoteConnected = false;

            this.name = Tools.getResolverName();
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

            #region Event Listeners
            MsgHandler.OnForwardLocalMessage += new forwardLocalMessage(ForwardLocalMessage);
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

            Console.WriteLine("Starting Custom Local Peer Resolver Service...");

            try
            {
                crs.Open();
                customResolver.Open();
                bRet = true;
                Console.WriteLine("Custom Local Peer Resolver Service is started");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in starting Custom Local Peer Resolver Service");
                Console.WriteLine(e);
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
            remoteMessageHandler.OnForwardRemoteMessage += new forwardRemoteMessage(ForwardRemoteMessage);

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

                    Console.WriteLine("Remote service started.");
                    Console.WriteLine("Connecting to {0}", h[n].IP);

                    //Retrieve Remote IP Addresses
                    foreach (var newRemote in remoteChannel.RetrieveContactList())                    
                    {
                        if (!h.Exists(delegate(RemoteHost x) { return x.netAddress == newRemote.netAddress; }))
                        {
                            h.Add(newRemote);
                            Tools.updateRemoteHosts(newRemote);
                        }
                    }

                    Console.WriteLine("Connected to: {0}", h[n].IP);
                    bRet = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in connecting to: {0}", h[n].IP);
                    Console.WriteLine(e); //For debug purpose
                    remoteHost.Abort();                    
                    n++;
                    bRet = false;
                }
            }

            return bRet;
        }

        //To Resolver
        private void ForwardLocalMessage(PeerMessage message)   
        {
            if (_messageList.Count > 0)
            {
                TransactionField t = getMessageByID(message.header.MessageID);
                
                if (t != null)
                {
                    message.header.Sender = t.peerName;
                    _messageList.Remove(t);
                }
            }

            remoteChannel.ManageRemoteMessages(message);
        }

        //From Resolver
        private void ForwardRemoteMessage(PeerMessage message)  
        {
            TransactionField t = new TransactionField();
            t.ID = message.header.MessageID;
            t.peerName = message.header.Sender;
            
            _messageList.Add(t);
            
            message.header.Sender = name;

            if (message.header.Receiver == "Resolver")
                message.header.Receiver = "@All";

            if (message is StatusNotifyMessage)
                Connector.channel.statusAdv((StatusNotifyMessage)message);
            else if (message is EnergyProposalMessage)
                Connector.channel.energyProposal((EnergyProposalMessage)message);
            else if (message is EnergyAcceptMessage)
                Connector.channel.acceptProposal((EnergyAcceptMessage)message);
            else if (message is EndProposalMessage)
                Connector.channel.endProposal((EndProposalMessage)message);
            else if (message is HeartBeatMessage)
                Connector.channel.heartBeat((HeartBeatMessage)message);
            else if (message is PeerIsDownMessage)
                Connector.channel.peerDown((PeerIsDownMessage)message);
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

            //Be polite! Send an HelloResponse
            Connector.channel.HelloResponse(MessageFactory.createHelloResponseMessage("@All", Tools.getResolverName(), Tools.getResolverName()));
        }

        private TransactionField getMessageByID(Guid ID)
        {
            TransactionField t = null;

            for (int i = 0; i < _messageList.Count; i++)
            {
                if (_messageList[i].ID == ID)
                {
                    t = _messageList[i];
                    break;
                }
            }

            return t;
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
                        //Remove the deadly peer but first alert the folks.
                        Connector.channel.peerDown(MessageFactory.createPeerIsDownMessage("@All", this.name, _buildings[i].Name));
                        remoteChannel.ManageRemoteMessages(MessageFactory.createPeerIsDownMessage("@All", this.name, _buildings[i].Name));
                        
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
            
        #endregion

        private class TransactionField
        {
            public Guid ID;
            public string peerName;
        }
    }
}
