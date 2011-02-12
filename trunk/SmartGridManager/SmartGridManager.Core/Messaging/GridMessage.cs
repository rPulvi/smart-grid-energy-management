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
        //TODO: sistemare messaggio
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

    [MessageContract]
    public class EnergyAcceptMessage : PeerMessage
    {
        [MessageBodyMember]
        public float energy { get; set; }

        public EnergyAcceptMessage() { }
    }

    [MessageContract]
    public class EndProposalMessage : PeerMessage
    {
        [MessageBodyMember]
        public Boolean endStatus { get; set; }

        public EndProposalMessage() { }
    }    
}
