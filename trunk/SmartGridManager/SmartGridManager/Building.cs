using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SmartGridManager
{
    class Building : Peer
    {
        private PowerManager _pwManager;
        private Thread peerthread;

        // TODO: Generare i costruttori per solo producer e solo consumer.

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="ID">An Unique ID</param>
        /// <param name="energy">The type of energy producted</param>
        /// <param name="energyThreshold">The threshold to become producer/consumer</param>
        public Building(string ID, EnergyType energy, float energyThreshold):base(ID)
        {
            _pwManager = new PowerManager(new EnergyGenerator(energy), energyThreshold);
            peerthread = new Thread(_pwManager.Start) { IsBackground = true };
            peerthread.Start();
        }

        public float getEnergyLevel()
        {
            return _pwManager.getEnergyLevel();            
        }

        public void StopEnergyProduction()
        {
            _pwManager.ShutDown();
        }

        public void setLevel(float value)
        {
            _pwManager.setLevel(value);
        }

    }
}
