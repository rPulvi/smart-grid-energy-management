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
    class Program
    {
        private CustomResolver crs = new CustomResolver { ControlShape = false };
        private ServiceHost customResolver;
        
        private ServiceHost remoteHost = new ServiceHost(typeof(PeerServices));
        private MessageHandler MsgHandler;

        static void Main(string[] args)
        {
            Program p = new Program();
            
            p.StartLocalResolver();
            p.StartRemoteConnection();

            Connector.Connect();
            p.MsgHandler = Connector.messageHandler;
            p.MsgHandler.OnRemoteAdv += new remoteAdv(p.SendRemoteRequest);

            Console.WriteLine("Press [ENTER] to exit.");
            Console.ReadLine();
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

            Console.WriteLine("Connecting to remote host..");

            h = Tools.getRemoteHosts();
            
            //To connect to remote host
            NetTcpBinding tcpBinding = new NetTcpBinding();
            EndpointAddress remoteEndpoint = new EndpointAddress(h[0].netAddress); //TODO: fix here.
            tcpBinding.Security.Mode = SecurityMode.None;
            
            ChannelFactory<IPeerServices> cf = new ChannelFactory<IPeerServices>(tcpBinding, remoteEndpoint);
            Connector.remoteChannel = cf.CreateChannel();

            try
            {
                remoteHost.Open();
                
                Console.WriteLine("Connected to: {0}",h[0].IP);
                //Retrieve Remote IP Addresses
                foreach (var newRemote in Connector.remoteChannel.RetrieveContactList())
                {                    
                    if (!h.Exists(delegate(RemoteHost x){ return x.netAddress == newRemote.netAddress;}))
                    {
                        h.Add(newRemote);
                        Tools.updateRemoteHosts(newRemote);
                    }
                }
                
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

            Connector.remoteChannel.ManageEnergyRequest(remEneReq);
        }
    }
}
