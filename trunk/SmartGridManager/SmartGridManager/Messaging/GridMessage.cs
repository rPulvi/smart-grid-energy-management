using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SmartGridManager.Messaging
{
    [MessageContract]
    public class GridMessage : PeerMessage
    {
        [MessageBodyMember]
        public String tmpField { get; set; }

        public GridMessage() { }
    }
}
