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

        [MessageBodyMember]
        public float energyReq { get; set; }

        public StatusNotifyMessage() { }
    }

    [MessageContract]
    public class EnergyProposalMessage : PeerMessage
    {
        [MessageBodyMember]
        public float price { get; set; }

        [MessageBodyMember]
        public float energyAvailable { get; set; }

        public EnergyProposalMessage() { }
    }

}
