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
    public class Resolver : Peer
    {
        private CustomResolver crs = new CustomResolver { ControlShape = false };
        private ServiceHost customResolver;
        private ServiceHost remoteHost;
        private IPeerServices remoteChannel;        
        
        private MessageHandler MsgHandler;
        private PeerServices remoteMessageHandler;

        private string _name;
        private PeerStatus _peerStatus;

        public Resolver(string name) : base(name) { 
            _name = name;
            _peerStatus = PeerStatus.Resolver;
            remoteMessageHandler.OnRemoteRequest += new remoteEnergyRequest(ManageRemoteRequest);

            StartLocalResolver();
            StartRemoteConnection();

            #region Normal Peer Activity

            Connector.Connect();
            MsgHandler = Connector.messageHandler;
            MsgHandler.OnRemoteAdv += new remoteAdv(SendRemoteRequest);

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
            List<RemoteHost> h;            
            
            remoteMessageHandler = new PeerServices();//----------------+
                                                      //                +
            remoteHost = new ServiceHost(remoteMessageHandler);//<------+
            h = Tools.getRemoteHosts();
            
            //To connect to remote host
            NetTcpBinding tcpBinding = new NetTcpBinding();
            EndpointAddress remoteEndpoint = new EndpointAddress(h[0].netAddress); //TODO: fix here.
            tcpBinding.Security.Mode = SecurityMode.None;                        
            
            ChannelFactory<IPeerServices> cf = new ChannelFactory<IPeerServices>(tcpBinding, remoteEndpoint);            
            remoteChannel = cf.CreateChannel();

            try
            {
                remoteHost.Open();

                Console.WriteLine("Remote service started.");
                Console.WriteLine("Connecting to {0}", h[0].IP);

                //Retrieve Remote IP Addresses
                foreach (var newRemote in remoteChannel.RetrieveContactList())
                {                    
                    if (!h.Exists(delegate(RemoteHost x){ return x.netAddress == newRemote.netAddress;}))
                    {
                        h.Add(newRemote);
                        Tools.updateRemoteHosts(newRemote);
                    }
                }

                Console.WriteLine("Connected to: {0}", h[0].IP);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in connecting to: {0}", h[0].IP);
                Console.WriteLine(e);
                remoteHost.Abort();
                Console.ReadLine();
            }
        }

        private void SendRemoteRequest(StatusNotifyMessage message)
        {
            RemoteEnergyRequest remEneReq = new RemoteEnergyRequest()
            {
                header = Tools.getHeader("@All", "resolver"),
                energyReq = message.energyReq
            };

            remoteChannel.ManageEnergyRequest(remEneReq);
        }

        private void ManageRemoteRequest(RemoteEnergyRequest message)
        {
            float enReq = message.energyReq;
            Connector.channel.statusAdv(MessageFactory.createEnergyRequestMessage(_name, _peerStatus, enReq));        
        }
    }
}
