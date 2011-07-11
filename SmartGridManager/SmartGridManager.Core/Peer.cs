using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using SmartGridManager.Core.Messaging;
using SmartGridManager.Core.Utils;
using SmartGridManager.Core.Commons;
using System.Runtime.Serialization;

namespace SmartGridManager.Core
{       
    public class Peer
    {
        public bool isConnected { get; private set; }
        public String ID { get; private set; }

        private PeerStatus status;        

        public Peer(String ID, PeerStatus status = PeerStatus.None)
        {
            this. isConnected = false;

            this.ID = ID;
            this.status = status;

            if (!(status == PeerStatus.Resolver)) //Il Resolver lancerà il servizio manualmente            
                this.StartService();
        }

        public void StartService()
        {
            if (Connector.Connect())
            {
                this.isConnected = true;
                XMLLogger.WriteLocalActivity("OnLine.");
            }
            else
            {
                this.isConnected = false;
                XMLLogger.WriteErrorMessage(this.GetType().FullName.ToString(), "Connection Error");                
            }
        }

        public void StopService() 
        {
            if (this.isConnected == true)
            {
                Connector.Disconnect();
                XMLLogger.WriteLocalActivity("OffLine.");
            }
        }

    }
}
