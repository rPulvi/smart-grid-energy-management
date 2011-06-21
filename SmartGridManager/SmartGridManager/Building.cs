using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SmartGridManager.Core;
using SmartGridManager.Core.Commons;
using SmartGridManager.Core.Messaging;
using System.Runtime.Serialization;
using SmartGridManager.Core.Utils;

namespace SmartGridManager
{
    public class Building : Peer
    {
        private PowerManager _pwManager;
        private Thread peerthread;

        private string _name;
        private EnergyType _enType;
        private PeerStatus _status;
        private float _enProduced;
        private float _enPeak;
        private float _price;
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
            

            _name = Name;
            _enType = enType;
            _enProduced = enProduced;
            _enPeak = energyPeak;
            _price = price;
            _address = address;
            _adminName = adminName;
            _status = status;

            _pwManager = new PowerManager(Name, status, new EnergyGenerator(enType, enProduced), energyPeak, price);
        }

        public void StopEnergyProduction()
        {
            _pwManager.ShutDown();
            base.StopService();            
        }

        public void Start()
        {
            peerthread = new Thread(_pwManager.Start) { IsBackground = true };
            peerthread.Start();

            XMLLogger.WriteLocalActivity("Power Manager Started. Sending Hello Message.");

            //send hello message
            Connector.channel.sayHello(MessageFactory.CreateHelloMessage("@All", _name, _status, _enType, _enProduced,
                _enPeak, _price, _address, _adminName));

            XMLLogger.WriteLocalActivity("Hello Message Sent.");
        }
    }
}
