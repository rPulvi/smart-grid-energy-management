using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.P2P;

namespace SmartGridManager.Core.Commons
{
    public class RemoteConnection
    {
        public RemoteHost remoteResolver;
    }

    public class IncomingConnection : RemoteConnection
    {        
        public Dictionary<Guid, EnergyLink> requests = new Dictionary<Guid, EnergyLink>();
    }

    public class OutgoingConnection : RemoteConnection
    {        
        public Dictionary<string, float> requests = new Dictionary<string, float>();
    }

    public enum ConnectionType
    {
        Incoming = 0,
        Outgoing = 1,
    }
}
