using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartGridManager
{
    public class EnergyLink
    {
        public string peerName { get; private set; }
        public float energy { get; private set; }

        public EnergyLink(string name, float en)
        {
            this.peerName = name;
            this.energy = en;
        }
    }    
}
