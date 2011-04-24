using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Core.Messaging;
using SmartGridManager.Core.Commons;
using SmartGridManager.Core.Utils;

namespace Resolver
{
    [ServiceContract]
    public interface IPeerServices
    {
        [OperationContract]
        void ManageEnergyRequest(RemoteEnergyRequest remoteReq);
        
        [OperationContract]
        List<RemoteHost> RetrieveContactList();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class PeerServices : IPeerServices
    {
        List<RemoteEnergyRequest> requestList = new List<RemoteEnergyRequest>();

        #region IPeerServices Members

        public void ManageEnergyRequest(RemoteEnergyRequest remoteReq)
        {
            requestList.Add(remoteReq);
        }

        public List<RemoteHost> RetrieveContactList()
        {
            return Tools.getRemoteHosts();
        }

        #endregion
    }
}
