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
        public static IPeerServices remoteChannel;

        public static MessageHandler messageHandler; //Static because we need to access the global context (InstanceContext), not a new object
        private static DuplexChannelFactory<IChannel> _factory;

        public static bool Connect()
        {            
            messageHandler = new MessageHandler();
            
            //create channel
            InstanceContext instanceContext = new InstanceContext(messageHandler);
            _factory = new DuplexChannelFactory<IChannel>(instanceContext, "GridEndpoint");            
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
                Console.WriteLine("that the service is running before executing this sample.  Refer to the readme");
                Console.WriteLine("for more details.");
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
