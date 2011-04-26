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
    public delegate void remoteEnergyRequest(RemoteEnergyRequest s);

    //[ServiceContract]
    [ServiceContract(CallbackContract = typeof(IPeerServices))]
    public interface IPeerServices
    {
        [OperationContract]
        void ManageEnergyRequest(RemoteEnergyRequest remoteReq);
        
        [OperationContract]
        List<RemoteHost> RetrieveContactList();
    }

    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class PeerServices : IPeerServices
    {
        public event remoteEnergyRequest OnRemoteRequest;
               
        public void ManageEnergyRequest(RemoteEnergyRequest remoteReq)
        {
            if (OnRemoteRequest != null)
                OnRemoteRequest(remoteReq);
        }

        public List<RemoteHost> RetrieveContactList()
        {
            return Tools.getRemoteHosts();
        }        
    }
}
