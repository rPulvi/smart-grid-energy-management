using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Core.Messaging;
using SmartGridManager.Core.Commons;
using SmartGridManager.Core.Utils;

namespace SmartGridManager.Core.P2P
{        
    public delegate void manageEnergyRequest(RemoteEnergyRequest m);
    public delegate void replyEnergyRequest(EndProposalMessage m);
    public delegate void remotePeerIsDown(PeerIsDownMessage m);

    [ServiceContract]
    //[ServiceContract(CallbackContract = typeof(IPeerServices))]
    public interface IPeerServices 
    {        
        [OperationContract]
        List<RemoteHost> RetrieveContactList();

        [OperationContract]
        void ManageRemoteEnergyRequest(RemoteEnergyRequest message);

        [OperationContract]
        void ReplyEnergyRequest(EndProposalMessage message);

        [OperationContract]
        void PeerDownAlert(PeerIsDownMessage message);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PeerServices : IPeerServices
    {        
        public event manageEnergyRequest OnRemoteEnergyRequest;
        public event replyEnergyRequest OnRemoteEnergyReply;
        public event remotePeerIsDown OnRemotePeerIsDown;

        public List<RemoteHost> RetrieveContactList()
        {
            return Tools.getRemoteHosts();
        }

        public void ManageRemoteEnergyRequest(RemoteEnergyRequest message)
        {
            if (OnRemoteEnergyRequest != null)
                OnRemoteEnergyRequest(message);
        }

        public void ReplyEnergyRequest(EndProposalMessage message)
        {
            if (OnRemoteEnergyReply != null)
                OnRemoteEnergyReply(message);
        }

        public void PeerDownAlert(PeerIsDownMessage message)
        {
            if (OnRemotePeerIsDown != null)
                OnRemotePeerIsDown(message);        
        }
    }
}
