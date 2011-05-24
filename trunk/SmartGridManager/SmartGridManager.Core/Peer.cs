﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using SmartGridManager.Core.Messaging;
using SmartGridManager.Core.Utils;
using SmartGridManager.Core.Commons;

namespace SmartGridManager.Core
{
    public class Peer
    {
        public String ID { get; private set; }
        private GridMessage _request;

        public Peer(String ID, PeerStatus status = PeerStatus.None)
        {
            this.ID = ID;

            if (!(status == PeerStatus.Resolver))
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

                Connector.channel.appendPeer(this);

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