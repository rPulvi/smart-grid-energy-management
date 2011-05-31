using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SmartGridManager.Core.Commons;
using SmartGridManager.Core;

namespace SmartGridManager
{
    class EnergyGenerator
    {
        #region members
        public EnergyType Type { get; private set; }
        public float EnergyLevel { get; set; }        
        #endregion

        #region Constructor
        public EnergyGenerator(EnergyType Type, float enProduced)
        {
            this.Type = Type;
            EnergyLevel = enProduced;
        }
        #endregion

        public void Stop()
        {            
            EnergyLevel = 0;
        }
    }
}
