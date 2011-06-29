using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.P2P;

namespace SmartGridManager.Core.Commons
{
    public class RemoteConnection
    {
    }

    public class IncomingConnection : RemoteConnection
    {
        public IRemote channel;
        public string remoteResolverName;
        public string IP;
        public string port;
        public Dictionary<Guid, EnergyLink> requests = new Dictionary<Guid, EnergyLink>();
    }

    public class OutgoingConnection : RemoteConnection
    {
        public string remoteResolverName;
        public string IP;
        public string port;
        public Dictionary<string, float> requests = new Dictionary<string, float>();
    }

    public enum ConnectionType
    {
        Incoming = 0,
        Outgoing = 1,
    }
}
