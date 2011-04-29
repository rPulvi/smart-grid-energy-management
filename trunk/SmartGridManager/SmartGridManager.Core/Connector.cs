using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Core.Messaging;
using SmartGridManager.Core.P2P;

namespace SmartGridManager.Core
{
    public interface IChannel : IMessages, IClientChannel
    {
    }

    public static class Connector
    {
        public static IChannel channel;         

        public static MessageHandler messageHandler; //Static because we need to access the global context (InstanceContext), not a new object        
        private static DuplexChannelFactory<IChannel> _factory;

        public static bool Connect()
        {
            //NetTcpBinding tcpBinding = new NetTcpBinding();
            NetPeerTcpBinding p2pBinding = new NetPeerTcpBinding();
            p2pBinding.Security.Mode = SecurityMode.None;
            EndpointAddress remoteEndpoint = new EndpointAddress(@"net.p2p://sucausca:8080/peerResolverService"); //TODO: fix here.
            
            //EndpointAddress remoteEndpoint = new EndpointAddress(@"net.p2p://localhost:8080/peerResolverService"); //TODO: fix here.

            messageHandler = new MessageHandler();
            
            //create channel
            InstanceContext instanceContext = new InstanceContext(messageHandler);
            //_factory = new DuplexChannelFactory<IChannel>(instanceContext, "GridEndpoint");
            _factory = new DuplexChannelFactory<IChannel>(instanceContext, p2pBinding, remoteEndpoint);
            channel = _factory.CreateChannel();
           
            //set filter
            PeerNode pn = ((IClientChannel)Connector.channel).GetProperty<PeerNode>();
            pn.MessagePropagationFilter = new RemoteOnlyMessagePropagationFilter();

            try
            {
                ((ICommunicationObject)channel).Open();
            }
            catch (CommunicationException ex){
                Console.WriteLine("Could not find resolver.  If you are using a custom resolver, please ensure");
                Console.WriteLine("that the service is running.");                
                Console.WriteLine("SystemError: {0}", ex);
                return false;
            }

            return true;            
        }

        public static void Disconnect()
        {
            channel.Close();
            if (_factory != null)
                _factory.Close();        
        }
    }
}
