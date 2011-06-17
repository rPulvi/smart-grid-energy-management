using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace SmartGridManager.Core.Utils
{
    public static class XMLLogger
    {
        private static XDocument xmlLog;

        public static void InitLogFile(string peerName)
        {            
            string logFileName = peerName + ".xml";

            if (!File.Exists (logFileName))
            {
                xmlLog = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("PeerLog",
                        new XAttribute("Name", peerName),
                    new XElement("LocalActivities"),
                    new XElement("RemoteActivities")));

                xmlLog.Save(logFileName);
            }

            xmlLog = XDocument.Load(logFileName);            
        }        
    }
}
