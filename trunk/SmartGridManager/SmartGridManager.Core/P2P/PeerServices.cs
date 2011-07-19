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
    public delegate void replyEnergyRequest(RemoteEndProposalMessage m);
    public delegate void remotePeerIsDown(PeerIsDownMessage m);

    [ServiceContract]    
    public interface IPeerServices 
    {        
        [OperationContract]
        List<RemoteHost> RetrieveContactList();

        [OperationContract]
        void ManageRemoteEnergyRequest(RemoteEnergyRequest message);

        [OperationContract]
        void ReplyEnergyRequest(RemoteEndProposalMessage message);

        [OperationContract]
        void PeerDownAlert(PeerIsDownMessage message);
    }

    public interface IRemote : IPeerServices, IClientChannel
    {
        //per avere la Open e la Close
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
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

        public void ReplyEnergyRequest(RemoteEndProposalMessage message)
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
