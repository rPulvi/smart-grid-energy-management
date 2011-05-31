using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SmartGridManager.Core;
using SmartGridManager.Core.Commons;
using SmartGridManager.Core.Messaging;
using System.Runtime.Serialization;

namespace SmartGridManager
{
    public class Building : Peer
    {
        private PowerManager _pwManager;
        private Thread peerthread;

        private string _address;
        private string _adminName;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="ID">An Unique ID</param>
        /// <param name="energy">The type of energy produced</param>
        /// <param name="energyPeak">The max energy consumed by a builiding</param>
        /// <param name="price">The energy selling price</param>
        public Building(String Name, PeerStatus status, EnergyType enType, float enProduced, float energyPeak, float price, string address, string adminName)
            : base(Name)
        {
            _address = address;
            _adminName = adminName;
            _pwManager = new PowerManager(Name, status, new EnergyGenerator(enType, enProduced), energyPeak, price);
            peerthread = new Thread(_pwManager.Start) { IsBackground = true };
            peerthread.Start();

            //send hello message
            Connector.channel.sayHello(MessageFactory.CreateHelloMessage("@All",Name,status,enType,enProduced,
                energyPeak,price,address,adminName));
        }

        public void StopEnergyProduction()
        {
            _pwManager.ShutDown();
        }
    }
}
