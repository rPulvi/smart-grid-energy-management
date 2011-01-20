using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace SmartGridManager
{

    public class Peer
    {
        private string _member;        
        private myMessage _request = new myMessage();
       
        public Peer(String name)
        {
            this._member = name;
            this.StartService();
        }

        public void StartService()
        {
            if (Connector.Connect())
            {
                _request.Name = "My name is " + _member;
                Console.WriteLine("Messaggio inviato da: {0}", _member);
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
