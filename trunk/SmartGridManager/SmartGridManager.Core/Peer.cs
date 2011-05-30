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
    [Serializable]
    [DataContract]
    public class Peer
    {
        [DataMember]
        public String ID { get; private set; }

        [DataMember]
        private GridMessage _request;

        [DataMember]
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
                //composing hello message
                _request = new GridMessage()
                {
                    header = Tools.getHeader("@All", this.ID, true),
                    descField = "PEER: " + this.ID + ".:: Hello ::."
                };

                //send hello message
                Connector.channel.sayHello(_request);

                if (!(status == PeerStatus.Resolver))
                    Connector.channel.appendPeer(MessageFactory.createAddPeerMessage(this.ID, this));

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
