using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SmartGridManager.Core;
using SmartGridManager.Core.Messaging;
using SmartGridManager.Core.Commons;

namespace SmartGridManager
{
    class PowerManager
    {
        private EnergyGenerator _generator;
        private StatusNotifyMessage _message;
        private MessageHandler MsgHandler;

        private Thread peerthread;
        private String _name;
        private PeerStatus _peerStatus;
        private float _enThreshold;        
        private Boolean _state;        

        public PowerManager(String bName, EnergyGenerator generator, float energyThreshold)
        {
            this.MsgHandler = Connector.messageHandler;
            MsgHandler.OnStatusChanged += new statusNotify(ManageStatus);            

            _generator = generator;
            _enThreshold = energyThreshold;
            _name = bName;
            _peerStatus = PeerStatus.Producer;
            _state = true;
        }

        public void Start()
        {
            peerthread = new Thread(_generator.Start) { IsBackground = true };
            peerthread.Start();

            while (_state == true)
            {
                if (getEnergyLevel() < _enThreshold)
                {
                    _peerStatus = PeerStatus.Consumer;
                    _message = new StatusNotifyMessage()
                    {
                        header = new StandardMessageHeader() { MessageID = Guid.NewGuid(), Receiver = "All", Sender = _name, TimeStamp = DateTime.Now },
                        status = _peerStatus
                    };
                    
                    Connector.channel.statusAdv(_message);
                }
            }
        }

        public void ShutDown()
        {            
            _generator.Stop();
            _state = false;
        }

        public float getEnergyLevel() { return _generator.EnergyLevel; }
        
        // TODO: Rinominare il metodo
        public void setLevel(float value)
        {
            _generator.level = value;
        }

        public void ManageStatus(PeerStatus s)
        {
            if (s == PeerStatus.Consumer)
            { 
                
            }
        }
    }
}
