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
    public delegate void forwardRemoteMessage(PeerMessage m);
    public delegate void manageEnergyRequest(StatusNotifyMessage m);
    public delegate void replyEnergyRequest(EndProposalMessage m);

    [ServiceContract]
    //[ServiceContract(CallbackContract = typeof(IPeerServices))]
    public interface IPeerServices 
    {        
        [OperationContract]
        List<RemoteHost> RetrieveContactList();

        [OperationContract]
        void ManageRemoteMessages(PeerMessage message);

        [OperationContract]
        void ManageRemoteEnergyRequest(StatusNotifyMessage message);

        [OperationContract]
        void ReplyEnergyRequest(EndProposalMessage message);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PeerServices : IPeerServices
    {
        public event forwardRemoteMessage OnForwardRemoteMessage;
        public event manageEnergyRequest OnRemoteEnergyRequest;
        public event replyEnergyRequest OnRemoteEnergyReply;

        public List<RemoteHost> RetrieveContactList()
        {
            return Tools.getRemoteHosts();
        }

        public void ManageRemoteMessages(PeerMessage message)
        {
            if (OnForwardRemoteMessage != null)
                OnForwardRemoteMessage(message);
        }

        public void ManageRemoteEnergyRequest(StatusNotifyMessage message)
        {
            if (OnRemoteEnergyRequest != null)
                OnRemoteEnergyRequest(message);
        }

        public void ReplyEnergyRequest(EndProposalMessage message)
        {
            if (OnRemoteEnergyReply != null)
                OnRemoteEnergyReply(message);
        }
    }
}
