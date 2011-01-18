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
        private float _enConsumed;
        public Thread peerthread;

        public PowerManager(EnergyGenerator generator, float energyConsumed)
        {
            _generator = generator;
            _enConsumed = energyConsumed;
            
           peerthread = new Thread(_generator.Start) { IsBackground = true };
            
           peerthread.Start();            
        }

        public void ShutDown()
        {
            //peerthread.Suspend();    
            _generator.Stop();
        }

        public float getEnergyLevel() { return _generator.EnergyLevel; }
    }
}
