﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Messaging;

namespace SmartGridManager
{
    public interface IChannel : IMessages, IClientChannel
    {
    }

    public static class Connector
    {
        public static IChannel channel;
        private static DuplexChannelFactory<IChannel> _factory;

        public static bool Connect()
        {
            InstanceContext instanceContext = new InstanceContext(new MessagesImplementation());
            _factory = new DuplexChannelFactory<IChannel>(instanceContext, "GridEndpoint");
            channel = _factory.CreateChannel();

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
