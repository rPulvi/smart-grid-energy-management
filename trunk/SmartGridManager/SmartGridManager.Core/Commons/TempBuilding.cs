using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartGridManager.Core.Commons
{
    public class TempBuilding
    {
        public string Name { get; set; }
        public PeerStatus status { get; set; }
        public EnergyType EnType { get; set; }
        public float EnProduced { get; set; }
        public float EnSold { get; set; }
        public float EnBought { get; set; }
        public float EnPeak { get; set; }
        public float EnPrice { get; set; }
        public string Address { get; set; }
        public string Admin { get; set; }
    }
}
