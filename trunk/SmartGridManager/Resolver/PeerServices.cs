using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Core.Messaging;

namespace Resolver
{

    [ServiceContract]
    public interface IPeerServices
    {
        [OperationContract]
        void ManageEnergyRequest(RemoteEnergyRequest remoteReq);
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

        #endregion
    }
}
