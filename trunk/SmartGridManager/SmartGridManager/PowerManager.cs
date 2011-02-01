using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SmartGridManager
{
    class PowerManager
    {
        private EnergyGenerator _generator;
        private float _enThreshold;
        private Thread peerthread;

        public PowerManager(EnergyGenerator generator, float energyThreshold)
        {
            _generator = generator;
            _enThreshold = energyThreshold;            
        }

        public void Start()
        {
            peerthread = new Thread(_generator.Start) { IsBackground = true };
            peerthread.Start();

            while (true)
            {
                if (getEnergyLevel() < _enThreshold)
                    Console.WriteLine("Sono Consumer");
                else
                    Console.WriteLine("Sono Producer");
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
