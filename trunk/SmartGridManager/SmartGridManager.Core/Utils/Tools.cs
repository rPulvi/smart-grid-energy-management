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
        private static Guid _MessageID;

        /// <summary>
        /// return a StandardMessageHeader generating a new message ID and applying the TimeStamp
        /// </summary>
        /// <param name="receiver">message receiver</param>
        /// <param name="sender">message sender</param>
        /// <returns>the message header</returns>
        public static StandardMessageHeader getHeader(String receiver, String sender, bool isNew=false )
        { 
            StandardMessageHeader m;            

            if (isNew == true)
                _MessageID = Guid.NewGuid();            

            m = new StandardMessageHeader{ 
                MessageID = _MessageID, 
                Receiver = receiver,
                Sender = sender,
                TimeStamp = DateTime.Now };

            return m;
        }

        public static List<RemoteHost> getRemoteHosts()
        {
            string address = @"net.tcp://";

            List<RemoteHost> hosts = new List<RemoteHost>();
            

            var remote = from r in XElement.Load("NetConfig.xml").Elements("Host")
                         select r;

            foreach (var host in remote)
            {
                RemoteHost h = new RemoteHost();

                h.IP = host.Element("IP").Value;
                h.port = host.Element("Port").Value;
                h.netAddress = address + h.IP + ":" + h.port + @"/Remote";

                hosts.Add(h);
            }

            return hosts;
        }

        public static void updateRemoteHosts(RemoteHost h)
        {
            XDocument xmlList = XDocument.Load("NetConfig.xml");

            xmlList.Element("RemoteHosts").Add(new XElement("Host", 
                                                            new XElement("IP",h.IP),
                                                            new XElement("Port",h.port))
                                                            );

            xmlList.Save("NetConfig.xml");
        }

        public static string getResolverName()
        {
            return XElement.Load("NetConfig.xml").Element("Name").Value;
        }
    }
}
