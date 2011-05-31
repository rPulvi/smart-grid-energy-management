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
        public String ID { get; private set; }

        private PeerStatus status;

        public Peer(String ID, PeerStatus status = PeerStatus.None)
        {
            this.ID = ID;
            this.status = status;

            if (!(status == PeerStatus.Resolver)) //Il Resolver lancerà il servizio manualmente
                this.StartService();
        }

        public void StartService()
        {
            if (Connector.Connect())
            {                                
                //handling Online/Offline events
                IOnlineStatus ostat = Connector.channel.GetProperty<IOnlineStatus>();

                ostat.Online += new EventHandler(OnOnline);
                ostat.Offline += new EventHandler(OnOffline);
            }
            else
                Console.WriteLine("Errore in connessione");
        }


        public void StopService() { Connector.Disconnect(); }

        // PeerNode event handlers
        static void OnOnline(object sender, EventArgs e)
        {
            Console.WriteLine("**  Online");
        }

        static void OnOffline(object sender, EventArgs e)
        {
            Console.WriteLine("**  Offline");
        }
    }
}
