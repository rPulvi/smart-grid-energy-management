using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SmartGridManager
{
    class EnergyGenerator
    {
        #region members
        public EnergyType Type { get; private set; }
        public float EnergyLevel { get; private set; }

        private Boolean _state;
        
        #endregion

        #region Constructor
        public EnergyGenerator(EnergyType e)
        {
            this.Type = e;
            _state = true;
        }
        #endregion

        public void Start()
        {
            float level = 0;

            switch (Type)
            {
                case EnergyType.Eolic:
                    level = 123;
                    break;

                case EnergyType.Hydric:
                    level = 50;
                    break;

                case EnergyType.Solar:
                    level = 789;
                    break;

                case EnergyType.Thermic:
                    level = 101;
                    break;
            }

            Random r = new Random();

            while (_state == true) 
            {                
                EnergyLevel = r.Next((int)level-5,(int)level+2);
                Thread.Sleep(1000);
            }
        }

        public void Stop()
        {
            _state = false;
            EnergyLevel = 0;
        }
    }
}
