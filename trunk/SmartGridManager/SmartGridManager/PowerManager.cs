using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SmartGridManager.Messaging;

namespace SmartGridManager
{
    class PowerManager
    {
        private EnergyGenerator _generator;
        private String _name;
        private float _enThreshold;
        private Thread peerthread;
        private AdvertisingMessage _message;

        public PowerManager(String bName, EnergyGenerator generator, float energyThreshold)
        {
            _generator = generator;
            _enThreshold = energyThreshold;
            _name = bName;
        }

        public void Start()
        {
            peerthread = new Thread(_generator.Start) { IsBackground = true };
            peerthread.Start();

            while (true)
            {
                if (getEnergyLevel() < _enThreshold)
                {
                    //Console.WriteLine("Sono Consumer");
                    _message = new AdvertisingMessage()
                    {
                        header = new StandardMessageHeader() { MessageID = Guid.NewGuid(), Receiver = "All", Sender = _name, TimeStamp = DateTime.Now },
                        status = "Sono Consumer"
                    };
                    Connector.channel.statusAdv(_message);
                }

                //else
                  //  Console.WriteLine("Sono Producer");
            }
        }

        public void ShutDown()
        {           
            _generator.Stop();
        }

        public float getEnergyLevel() { return _generator.EnergyLevel; }
        
        // TODO: Rinominare il metodo
        public void setLevel(float value)
        {
            _generator.level = value;
        }
    }
}
