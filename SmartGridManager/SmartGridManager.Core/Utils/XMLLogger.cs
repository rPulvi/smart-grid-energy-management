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

        public static void WriteLocalActivity(string logMessage)
        {
            xmlLog = XDocument.Load("log.xml");

            try
            {
                xmlLog.Root.Element("LocalActivities").Add(
                    new XElement("LogMessage",
                    new XElement("Time", DateTime.Now),
                    new XElement("Message", logMessage)
                    ));

                xmlLog.Save("log.xml");
            }
            catch(Exception e)
            {
                Console.WriteLine(" XMLLogger Local - Error in Writing {0} : " + e, logMessage);
            }
        }

        public static void WriteRemoteActivity(string logMessage)
        {
            xmlLog = XDocument.Load("log.xml");

            try
            {
                xmlLog.Root.Element("RemoteActivities").Add(
                    new XElement("LogMessage",
                    new XElement("Time", DateTime.Now),
                    new XElement("Message", logMessage)
                    ));

                xmlLog.Save("log.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine(" XMLLogger Remote - Error in Writing {0} : " + e, logMessage);
            }
        }

        public static void WriteErrorMessage(string className, string logMessage)
        {
            xmlLog = XDocument.Load("log.xml");

            try
            {
                xmlLog.Root.Element("ErrorMessages").Add(
					new XElement("LogMessage",
                    new XElement("Time", DateTime.Now),
                    new XElement("Class", className),
                    new XElement("Error", logMessage)
                    ));

                xmlLog.Save("log.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine(" XMLLogger Error Section - Error in Writing {0} : " + e, logMessage);
            }
        }
    }
}
