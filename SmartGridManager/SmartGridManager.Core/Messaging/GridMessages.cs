using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Core.Commons;

//This file contains all the contracts of the messages used in the Grid

namespace SmartGridManager.Core.Messaging
{
    [MessageContract]
    public class HelloMessage : PeerMessage
    {
        [MessageBodyMember]
        public PeerStatus Status { get; set; }

        [MessageBodyMember]
        public EnergyType EnType { get; set; }

        [MessageBodyMember]
        public float EnProduced { get; set; }

        [MessageBodyMember]
        public float EnPeak { get; set; }

        [MessageBodyMember]
        public float EnPrice { get; set; }

        [MessageBodyMember]
        public string Address { get; set; }

        [MessageBodyMember]
        public string Admin { get; set; }
    }
    
    [MessageContract]
    public class HelloResponseMessage : PeerMessage
    {
        [MessageBodyMember]
        public String ResolverName { get; set; }

        public HelloResponseMessage() { }
    }

    [MessageContract]
    public class GridMessage : PeerMessage
    {
        [MessageBodyMember]
        public String descField { get; set; }

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
        public string peerName { get; set; }

        [MessageBodyMember]
        public float energy { get; set; }

        public EnergyAcceptMessage() { }
    }

    [MessageContract]
    public class EndProposalMessage : PeerMessage
    {
        [MessageBodyMember]
        public Boolean endStatus { get; set; }

        [MessageBodyMember]
        public float energy { get; set; }

        [MessageBodyMember]
        public float price { get; set; }

        public EndProposalMessage() { }
    }

    [MessageContract]
    public class HeartBeatMessage : PeerMessage
    {
        public HeartBeatMessage() { }
    }

    [MessageContract]
    public class UpdateStatusMessage : PeerMessage
    {
        [MessageBodyMember]
        public float energySold { get; set; }
        
        [MessageBodyMember]
        public float energyBought { get; set; }
        
        public UpdateStatusMessage() { }
    }

    [MessageContract]
    public class PeerIsDownMessage : PeerMessage
    {
        [MessageBodyMember]
        public string peerName { get; set; }

        public PeerIsDownMessage() { }
    }

    [MessageContract]
    public class RemoteEnergyRequest : PeerMessage
    {
        [MessageBodyMember]
        public StatusNotifyMessage enReqMessage {get; set;}

        [MessageBodyMember]
        public string IP { get; set; }

        [MessageBodyMember]
        public string port { get; set; }

        public RemoteEnergyRequest() { }
    }

    [MessageContract]
    public class RemoteEndProposalMessage : PeerMessage
    {
        [MessageBodyMember]
        public EndProposalMessage endProposalMessage { get; set; }

        [MessageBodyMember]
        public string IP { get; set; }

        [MessageBodyMember]
        public string port { get; set; }

        public RemoteEndProposalMessage() { }
    }
}
