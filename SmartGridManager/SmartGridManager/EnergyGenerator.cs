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
        public EnergyGenerator(EnergyType Type)
        {
            switch (Type)
            {
                case EnergyType.Eolic:
                    EnergyLevel = 123;
                    break;

                case EnergyType.Hydric:
                    EnergyLevel = 50;
                    break;

                case EnergyType.Solar:
                    EnergyLevel = 789;
                    break;

                case EnergyType.Thermic:
                    EnergyLevel = 101;
                    break;
            }            
        }
        #endregion

        public void Stop()
        {            
            EnergyLevel = 0;
        }
    }
}
