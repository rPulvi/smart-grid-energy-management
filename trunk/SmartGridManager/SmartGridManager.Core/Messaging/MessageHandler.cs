using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.Commons;

namespace SmartGridManager.Core.Messaging
{
    #region delegates
    public delegate void sayHello (HelloMessage m);
    public delegate void HelloResponse (HelloResponseMessage m);
    public delegate void statusNotify (StatusNotifyMessage m);
    public delegate void energyProposal (EnergyProposalMessage m);
    public delegate void acceptProposal (EnergyAcceptMessage m);
    public delegate void endProposal (EndProposalMessage m);
    public delegate void heartBeat (HeartBeatMessage m);
    public delegate void remoteAdv (StatusNotifyMessage m);
    public delegate void forwardLocalMessage (PeerMessage m);
    public delegate void updateStatus (UpdateStatusMessage m);
    #endregion

    public class MessageHandler : MessagesImplementation
    {
        #region events
        public event sayHello OnSayHello;
        public event HelloResponse OnHelloResponse;
        public event statusNotify OnStatusChanged;
        public event energyProposal OnProposalArrived;
        public event acceptProposal OnProposalAccepted;
        public event endProposal OnEndProposalArrived;
        public event heartBeat OnHeartBeat;
        public event remoteAdv OnRemoteAdv;
        public event forwardLocalMessage OnForwardLocalMessage;
        public event updateStatus OnUpdateStatus;
        #endregion

        public override void sayHello(HelloMessage message)
        {
            if (OnSayHello != null)
                OnSayHello(message);

            base.sayHello(message);
        }

        public override void HelloResponse(HelloResponseMessage message)
        {
            if (OnHelloResponse != null)
                OnHelloResponse(message);

            base.HelloResponse(message);
        }

        public override void statusAdv(StatusNotifyMessage message)
        {
            if (OnStatusChanged != null)
                OnStatusChanged(message);

            base.statusAdv(message);
        }

        public override void energyProposal(EnergyProposalMessage message)
        {
            if (OnProposalArrived != null)
                OnProposalArrived(message);

            base.energyProposal(message);
        }

        public override void acceptProposal(EnergyAcceptMessage message)
        {
            if (OnProposalAccepted != null)
                OnProposalAccepted(message);

            base.acceptProposal(message);
        }

        public override void endProposal(EndProposalMessage message)
        {
            if (OnEndProposalArrived != null)
                OnEndProposalArrived(message);

            base.endProposal(message);
        }

        public override void heartBeat(HeartBeatMessage message)
        {
            if (OnHeartBeat != null)
                OnHeartBeat(message);

            base.heartBeat(message);
        }

        public override void remoteAdv(StatusNotifyMessage message)
        {
            if (OnRemoteAdv != null)
                OnRemoteAdv(message);
            
            base.remoteAdv(message);
        }

        public override void forwardLocalMessage(PeerMessage message)
        {
            if (OnForwardLocalMessage != null)
                OnForwardLocalMessage(message);
            
            base.forwardLocalMessage(message);
        }

        public override void updateEnergyStatus(UpdateStatusMessage message)
        {
            if (OnUpdateStatus != null)
                OnUpdateStatus(message);

            base.updateEnergyStatus(message);
        }
    }
}
