using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.Messaging;
using SmartGridManager.Core.Commons;
using System.Xml;
using System.Xml.Linq;


namespace SmartGridManager.Core.Utils
{
    public static class Tools
    {
        /// <summary>
        /// return a StandardMessageHeader generating a new message ID and applying the TimeStamp
        /// </summary>
        /// <param name="receiver">message receiver</param>
        /// <param name="sender">message sender</param>
        /// <returns>the message header</returns>
        public static StandardMessageHeader getHeader(String receiver, String sender )
        { 
            StandardMessageHeader m;

            m = new StandardMessageHeader{ 
                MessageID = Guid.NewGuid(), 
                Receiver = receiver, 
                Sender = sender, 
                TimeStamp = DateTime.Now };

            return m;
        }

        public static List<RemoteHost> getRemoteHosts()
        {
            string address = @"net.tcp://";

            List<RemoteHost> hosts = new List<RemoteHost>();
            RemoteHost h = new RemoteHost();

            var remote = from r in XElement.Load("NetConfig.xml").Elements("Host")
                         select r;

            foreach (var host in remote)
            {
                h.IP = host.Element("IP").Value;
                h.port = host.Element("Port").Value;
                h.netAddress = address + h.IP + ":" + h.port + @"/Remote";

                hosts.Add(h);
            }

            return hosts;
        }
    }
}
