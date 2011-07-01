using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.P2P;

namespace SmartGridManager.Core.Commons
{
    public class RemoteConnection
    {
        public ConnectionType type;
        public RemoteHost remoteResolver;
        public Dictionary<Guid, RemoteRequest> requests = new Dictionary<Guid, RemoteRequest>();
    }
   
    public enum ConnectionType
    {
        Incoming = 0,
        Outgoing = 1,
    }
}
