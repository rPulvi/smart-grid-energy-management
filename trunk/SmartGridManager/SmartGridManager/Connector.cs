using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SmartGridManager
{
    public static class Connector
    {
        public static ITestChannel channel;
        private static DuplexChannelFactory<ITestChannel> _factory;

        public static bool Connect()
        {
            InstanceContext instanceContext = new InstanceContext(new TestImplementation());
            _factory = new DuplexChannelFactory<ITestChannel>(instanceContext, "TestEndpoint");
            channel = _factory.CreateChannel();

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
