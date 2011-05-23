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

namespace Resolver
{
    public interface IRemote : IPeerServices, IClientChannel
    { 
    
    }

    public class Resolver : Peer
    {
        private CustomResolver crs = new CustomResolver { ControlShape = false };
        private ServiceHost customResolver;
        
        private ServiceHost remoteHost;
        //private IPeerServices remoteChannel;
        private IRemote remoteChannel;
        
        private MessageHandler MsgHandler;
        private PeerServices remoteMessageHandler;

        public string name { get; private set; }
        
        private PeerStatus _peerStatus;     

        private List<EnergyProposalMessage> _proposalList = new List<EnergyProposalMessage>();
        private Dictionary<Guid, String> _messageList = new Dictionary<Guid, String>();

        public Resolver() : base(Tools.getResolverName(),PeerStatus.Resolver){
            this.name = Tools.getResolverName();
            this._peerStatus = PeerStatus.Resolver;
        }

        public void Connect()
        {
            StartLocalResolver();
            StartRemoteConnection();                
            
            remoteMessageHandler.OnForwardRemoteMessage += new forwardRemoteMessage(ForwardRemoteMessage);

            #region Normal Peer Activity

            base.StartService();
            MsgHandler = Connector.messageHandler;
            
            MsgHandler.OnForwardLocalMessage += new forwardLocalMessage(ForwardLocalMessage);
            MsgHandler.OnSayHello += new sayHello(HelloResponse);

            #endregion
        }

        private void StartLocalResolver()
        {            
            customResolver = new ServiceHost(crs);

            Console.WriteLine("Starting Custom Local Peer Resolver Service...");

            try
            {
                crs.Open();
                customResolver.Open();
                Console.WriteLine("Custom Local Peer Resolver Service is started");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in starting Custom Local Peer Resolver Service");
                Console.WriteLine(e);
                crs.Close();                
                customResolver.Abort();
                Console.ReadLine();
            }        
        }

        private void StartRemoteConnection()
        {
            int n=0;
            bool bRet = false;
            List<RemoteHost> h;            
            
            remoteMessageHandler = new PeerServices();

            remoteHost = new ServiceHost(remoteMessageHandler);
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
                    Console.WriteLine(e); //For debug
                    remoteHost.Abort();
                    Console.ReadLine();
                    n++;
                    bRet = false;
                }
            }            
        }

        //To Resolver
        private void ForwardLocalMessage(PeerMessage message)   
        {
            message.header.Sender = getNameByID(message.header.MessageID);
            _messageList.Remove(message.header.MessageID);

            remoteChannel.ManageRemoteMessages(message);            
        }

        //From Resolver
        private void ForwardRemoteMessage(PeerMessage message)  
        {
            _messageList.Add(message.header.MessageID, message.header.Sender);
            
            message.header.Sender = name;

            if (message.header.Receiver == "Resolver")
                message.header.Receiver = "@all";

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
        }

        private void HelloResponse(String s)
        {
            MessageFactory.createHelloResponseMessage("@All", Tools.getResolverName(), Tools.getResolverName());
        }

        private string getNameByID(Guid ID)
        { 
            string name = "";

            if (_messageList.ContainsKey((Guid)ID))
            {
                foreach (var k in _messageList)
                {
                    if (k.Key == ID)
                    {
                        name = k.Value;
                        break;
                    }
                }
            }

            return name;
        }
    }
}
