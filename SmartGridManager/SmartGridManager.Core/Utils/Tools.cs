using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.Messaging;
using SmartGridManager.Core.Commons;
using System.Xml;
using System.Xml.Linq;
using System.Net;


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
        public static StandardMessageHeader getHeader(String receiver, String sender)
        { 
            StandardMessageHeader m;                                   

            m = new StandardMessageHeader{
                MessageID = Guid.NewGuid(), 
                Receiver = receiver,
                Sender = sender,
                TimeStamp = DateTime.Now };

            return m;
        }

        public static StandardMessageHeader getHeader(String receiver, String sender, Guid SessionMessageID)
        {
            StandardMessageHeader m;

            m = new StandardMessageHeader
            {
                MessageID = SessionMessageID,
                Receiver = receiver,
                Sender = sender,
                TimeStamp = DateTime.Now
            };

            return m;
        }

        public static List<RemoteHost> getRemoteHosts()
        {
            string address = @"net.tcp://";

            List<RemoteHost> hosts = new List<RemoteHost>();
            

            var remote = from r in XElement.Load("NetConfig.xml").Element("RemoteHosts").Elements("Host")
                         select r;

            foreach (var host in remote)
            {
                RemoteHost h = new RemoteHost();

                h.name = host.Element("Name").Value;
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

            xmlList.Element("Params").Element("RemoteHosts").Add(new XElement("Host",
                                                            new XElement("Name",h.name),
                                                            new XElement("IP",h.IP),
                                                            new XElement("Port",h.port))
                                                            );

            xmlList.Save("NetConfig.xml");
        }

        public static string getResolverName()
        {
            return XElement.Load("NetConfig.xml").Element("Name").Value;
        }

        public static string getResolverServicePort()
        {
            return XElement.Load("NetConfig.xml").Element("ServicePort").Value;
        }

        public static string getLocalIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            string IPAddress = "0.0.0.0";

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                    IPAddress = ip.ToString();
            }

            return IPAddress;
        }
    }
}
