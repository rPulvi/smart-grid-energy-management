using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartGridManager
{
    class PowerManager
    {
        private EnergyGenerator _generator;
        private float _enConsumed;

        public PowerManager(EnergyGenerator generator, float energyConsumed)
        {
            _generator = generator;
            _enConsumed = energyConsumed;
            _generator.Start();
        }

        public float getEnergyLevel() { return _generator.EnergyLevel; }
    }
}
