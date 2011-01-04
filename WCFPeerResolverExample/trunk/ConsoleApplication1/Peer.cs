using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Net.PeerToPeer;

namespace ConsoleApplication1
{
    [SerializableAttribute]
    class Peer : PeerName
    {
        public string ID { get; private set; }

        public IMessagePassing Channel;
        public IMessagePassing Host;

        private DuplexChannelFactory<IMessagePassing> _factory;
        private readonly AutoResetEvent _stopFlag = new AutoResetEvent(false);

        public Peer(string id)
            : base(id, PeerNameType.Unsecured)
        {
            ID = id;
        }

        public void StartService()
        {
            var binding = new NetPeerTcpBinding();
            binding.Security.Mode = SecurityMode.None;

            var endpoint = new ServiceEndpoint(
                ContractDescription.GetContract(typeof(IMessagePassing)),
                binding,
                new EndpointAddress("net.p2p://Messagerie"));

            Host = new MessagePassing();

            _factory = new DuplexChannelFactory<IMessagePassing>
                (new InstanceContext(Host), 
                endpoint);

            var channel = _factory.CreateChannel();

            ((ICommunicationObject)channel).Open();

            Channel = channel;

        }


        public void StopService()
        {
            ((ICommunicationObject)Channel).Close();
            if (_factory != null)
                _factory.Close();
        }

        public void Run()
        {
            Console.WriteLine("[Starting Service]");
            StartService();

            Console.WriteLine("[Service Started]");
            _stopFlag.WaitOne();

            Console.WriteLine("[Stopping Service]");
            StopService();
            
            Console.WriteLine("[Service Stopped]");
        }

        public void Stop()
        {
            _stopFlag.Set();
        }

    }
}
