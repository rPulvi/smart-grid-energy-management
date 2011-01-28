using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace SmartGridManager.Messaging
{
    ///<summary>
    /// This is the standard template for a message.
    /// </summary>
    //[DataContract]
    [MessageContract]
    public class PeerMessage
    {
        ///<summary>
        /// Message header
        ///</summary>

        //[DataMember]
        [MessageHeader]
        public StandardMessageHeader header { get; set; }
    }

    //[MessageContract]
    [DataContract]
    public class StandardMessageHeader
    {
        /// <summary>
        /// Message creation time
        /// </summary>
        //[MessageHeader]
        [DataMember]
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// A global unique identifier for the message
        /// </summary>
        //[MessageHeader]

        [DataMember]
        public Guid MessageID { get; set; }

        /// <summary>
        /// The sender of the message
        /// </summary>
        //[MessageHeader]
        [DataMember]
        public String Sender { get; set; }

        /// <summary>
        /// The destination of the message
        /// </summary>
        //[MessageHeader]
        [DataMember]
        public String Receiver { get; set; }

        public StandardMessageHeader() { }
    }
}
