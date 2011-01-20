using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace SmartGridManager
{

    public class Peer
    {
        private string _member;
        private ITestChannel _channel;
        private myMessage _request = new myMessage();
        DuplexChannelFactory<ITestChannel> _factory;

        public Peer(String name)
        {
            this._member = name;
            this.StartService();
        }

        public void StartService()
        {
            InstanceContext instanceContext = new InstanceContext(new TestImplementation());
            _factory = new DuplexChannelFactory<ITestChannel>(instanceContext, "TestEndpoint");
            _channel = _factory.CreateChannel();
            
            IOnlineStatus ostat = _channel.GetProperty<IOnlineStatus>();
            
            ostat.Online += new EventHandler(OnOnline);
            ostat.Offline += new EventHandler(OnOffline);

            try
            {
                ((ICommunicationObject)_channel).Open();
                _request.Name = "My name is " + _member;
                Console.WriteLine("Messaggio inviato da: {0}", _member);
                _channel.testFunction(_request);
            }
            catch(CommunicationException)
            {
                Console.WriteLine("Could not find resolver.  If you are using a custom resolver, please ensure");
                Console.WriteLine("that the service is running before executing this sample.  Refer to the readme");
                Console.WriteLine("for more details.");
                return;
            }

        }

        public void StopService()
        {
            _channel.Close();
            if (_factory != null)
                _factory.Close();
        }

        // PeerNode event handlers
        static void OnOnline(object sender, EventArgs e)
        {
            Console.WriteLine("**  Online");
        }

        static void OnOffline(object sender, EventArgs e)
        {                  
            Console.WriteLine("**  Offline");
        }        
    }
}
