using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartGridManager.Core.Commons
{
    public class RemoteRequest
    {
        public string localePeerName { get; set; }
        public string remotePeerName { get; set; }
        public float energy { get; set; }
    }
}
