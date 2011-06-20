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
        private static string logFileName;

        public static void InitLogFile(string peerName)
        {            
            logFileName = "log.xml";

            if (!File.Exists (logFileName))
            {
                xmlLog = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("PeerLog",
                        new XAttribute("Name", peerName),
                    new XElement("LocalActivities"),
                    new XElement("RemoteActivities"),
                    new XElement("ErrorMessages")
                    ));

                xmlLog.Save(logFileName);
            }

            xmlLog = XDocument.Load(logFileName);            
        }

        public static void WriteLocalActivity(string logMessage)
        {
            try
            {
                xmlLog.Root.Element("LocalActivities").Add(
                    new XElement("LogMessage",
                    new XElement("Time", DateTime.Now),
                    new XElement("Message", logMessage)
                    ));

                xmlLog.Save(logFileName);
            }
            catch(Exception e)
            {
                Console.WriteLine(" XMLLogger Local - Error in Writing {0} : " + e, logMessage);
            }
        }

        public static void WriteRemoteActivity(string logMessage)
        {
            try
            {
                xmlLog.Root.Element("RemoteActivities").Add(
                    new XElement("Time", DateTime.Now),
                    new XElement("Message", logMessage)
                    );

                xmlLog.Save(logFileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(" XMLLogger Remote - Error in Writing {0} : " + e, logMessage);
            }
        }

        public static void WriteErrorMessage(string className, string logMessage)
        {
            try
            {
                xmlLog.Root.Element("ErrorMessages").Add(
                    new XElement("Time", DateTime.Now),
                    new XElement("Class", className),
                    new XElement("Error", logMessage)
                    );

                xmlLog.Save(logFileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(" XMLLogger Error Section - Error in Writing {0} : " + e, logMessage);
            }
        }
    }
}
