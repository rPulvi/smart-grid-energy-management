﻿using System;
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
    public delegate void forwardRemoteMessage(PeerMessage m);

    [ServiceContract]
    //[ServiceContract(CallbackContract = typeof(IPeerServices))]
    public interface IPeerServices
    {
        [OperationContract]
        void ManageEnergyRequest(RemoteEnergyRequest remoteReq);
        
        [OperationContract]
        List<RemoteHost> RetrieveContactList();

        [OperationContract]
        void ManageRemoteMessages(PeerMessage message);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PeerServices : IPeerServices
    {
        public event remoteEnergyRequest OnRemoteRequest;
        public event forwardRemoteMessage OnForwardRemoteMessage;

        public void ManageEnergyRequest(RemoteEnergyRequest remoteReq)
        {
            if (OnRemoteRequest != null)
                OnRemoteRequest(remoteReq);
        }

        public List<RemoteHost> RetrieveContactList()
        {
            return Tools.getRemoteHosts();
        }

        public void ManageRemoteMessages(PeerMessage message)
        {
            if (OnForwardRemoteMessage != null)
                OnForwardRemoteMessage(message);
        }
    }
}
