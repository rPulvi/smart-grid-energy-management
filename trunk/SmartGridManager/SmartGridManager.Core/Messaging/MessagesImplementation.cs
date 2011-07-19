using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Core.Commons;
using SmartGridManager.Core.Utils;

namespace SmartGridManager.Core.Messaging
{
    /*
     * This class was intentionally left blank for further implementation.
     */

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class MessagesImplementation : IMessages
    {
        public virtual void sayHello(HelloMessage message)
        {
            //foo;
        }

        public virtual void HelloResponse(HelloResponseMessage message)
        { 
            //foo;
        }

        public virtual void statusAdv(StatusNotifyMessage message)
        {            
            //foo;
        }

        public virtual void energyProposal(EnergyProposalMessage message)
        {
            //foo; 
        }

        public virtual void acceptProposal(EnergyAcceptMessage message)
        {
            //foo;
        }

        public virtual void endProposal(EndProposalMessage message)
        {
            //foo;         
        }

        public virtual void heartBeat(HeartBeatMessage message)
        {            
            //foo;
        }

        public virtual void remoteAdv(StatusNotifyMessage message)
        {
            //foo;
        }

        public virtual void forwardEnergyRequest(StatusNotifyMessage message)
        {
            //foo;
        }

        public virtual void forwardEnergyReply(EndProposalMessage message)
        {
            //foo;
        }

        public virtual void updateEnergyStatus(UpdateStatusMessage message)
        {
            //foo;
        }

        public virtual void peerDown(PeerIsDownMessage message)
        {
            XMLLogger.WriteRemoteActivity("Peer " +  message.peerName + " is down");
        }
    }
}
