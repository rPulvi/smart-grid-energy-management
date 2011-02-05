using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Core.Commons;

namespace SmartGridManager.Core.Messaging
{
    [MessageContract]
    public class GridMessage : PeerMessage
    {
        [MessageBodyMember]
        public String tmpField { get; set; }

        public GridMessage() { }
    }
    
    [MessageContract]
    public class StatusNotifyMessage : PeerMessage
    {
        [MessageBodyMember]
        public PeerStatus status { get; set; }

        public StatusNotifyMessage() { }
    }
}
