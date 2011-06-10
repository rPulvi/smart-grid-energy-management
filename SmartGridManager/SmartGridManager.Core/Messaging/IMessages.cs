using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SmartGridManager.Core.Messaging
{
    [ServiceContract(CallbackContract = typeof(IMessages))]
    public interface IMessages
    {
        [OperationContract(IsOneWay = true)]
        void sayHello(HelloMessage m);

        [OperationContract(IsOneWay = true)]
        void HelloResponse(HelloResponseMessage m);

        [OperationContract(IsOneWay = true)]
        void statusAdv(StatusNotifyMessage m);

        [OperationContract(IsOneWay = true)]
        void energyProposal(EnergyProposalMessage m);

        [OperationContract(IsOneWay = true)]
        void acceptProposal(EnergyAcceptMessage m);

        [OperationContract(IsOneWay = true)]
        void endProposal(EndProposalMessage m);

        [OperationContract(IsOneWay = true)]
        void heartBeat(HeartBeatMessage m);

        [OperationContract(IsOneWay = true)]
        void remoteAdv(StatusNotifyMessage m);

        [OperationContract(IsOneWay = true)]
        void forwardLocalMessage(PeerMessage m);

        [OperationContract(IsOneWay = true)]
        void updateEnergyStatus(UpdateStatusMessage m);

        [OperationContract(IsOneWay = true)]
        void peerDown(PeerIsDownMessage m);

    }
}
