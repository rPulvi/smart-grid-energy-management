﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using SmartGridManager.Core.Messaging;
using SmartGridManager.Core.Utils;

namespace SmartGridManager.Core
{
    public class Peer
    {
        private String _ID;                
        private GridMessage _request;

        public Peer(String ID)
        {
            this._ID = ID;
            this.StartService();
        }

        public void StartService()
        {
            if (Connector.Connect())
            {
                _request = new GridMessage()
                {
                    header = Tools.getHeader("Tu", this._ID),
                    tmpField = "Se leggi questo vuol dire che funziona"
                };
                
                Console.WriteLine("Messaggio inviato da: {0}", _ID);
                
                Connector.channel.sayHello(_request);
            }
            else
                Console.WriteLine("Errore in connessione");

            IOnlineStatus ostat = Connector.channel.GetProperty<IOnlineStatus>();

            ostat.Online += new EventHandler(OnOnline);
            ostat.Offline += new EventHandler(OnOffline);
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
