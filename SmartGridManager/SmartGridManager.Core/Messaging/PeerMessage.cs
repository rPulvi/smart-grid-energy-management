using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace SmartGridManager.Core.Messaging
{
    [MessageContract]
    public class PeerMessage
    {
        [MessageHeader]
        public StandardMessageHeader header { get; set; }
    }


    [DataContract]
    public class StandardMessageHeader
    {
        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public Guid MessageID { get; set; }

        [DataMember]
        public String Sender { get; set; }

        [DataMember]
        public String Receiver { get; set; }

        public StandardMessageHeader() { }
    }
}
