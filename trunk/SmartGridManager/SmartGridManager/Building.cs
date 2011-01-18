using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartGridManager
{
    class Building : Peer
    {
        private PowerManager _pwManager;        

        public Building(string ID, EnergyType energy, float energyConsumed):base(ID)
        {            
            _pwManager = new PowerManager(new EnergyGenerator(energy), energyConsumed);
        }

        public float getEnergyLevel()
        {
            return _pwManager.getEnergyLevel();            
        }

        public void StopEnergyProduction()
        {
            _pwManager.ShutDown();
        }
    }
}
