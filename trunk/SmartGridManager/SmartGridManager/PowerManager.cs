using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SmartGridManager.Core;
using SmartGridManager.Core.Messaging;
using SmartGridManager.Core.Commons;
using SmartGridManager.Core.Utils;

namespace SmartGridManager
{
    class PowerManager
    {

        #region Attributes
        private EnergyGenerator _generator;        
        private MessageHandler MsgHandler;        

        private Thread peerthread;
        private String _name;
        private PeerStatus _peerStatus;
        private float _enThreshold;        
        private Boolean _state;  // TODO: cambiare nome      
        #endregion

        public PowerManager(String bName, EnergyGenerator generator, float energyThreshold)
        {
            this.MsgHandler = Connector.messageHandler;
            
            MsgHandler.OnStatusChanged += new statusNotify(ManageStatus);
            MsgHandler.OnProposalArrived += new energyProposal(EvaluateProposal);

            _generator = generator;
            _enThreshold = energyThreshold;
            _name = bName;
            _peerStatus = PeerStatus.Producer;
            _state = true;
        }

        public void Start()
        {
            Boolean messageSent = false;

            peerthread = new Thread(_generator.Start) { IsBackground = true };
            peerthread.Start();

            while (_state == true)
            {
                //Check the energy level
                if (getEnergyLevel() < _enThreshold)
                {
                    _peerStatus = PeerStatus.Consumer;

                    if (messageSent == false)
                    {
                        StatusNotifyMessage notifyMessage = new StatusNotifyMessage()
                        {
                            header = Tools.getHeader("All", _name),
                            status = _peerStatus,
                            energyReq = _enThreshold - getEnergyLevel()
                        };

                        Connector.channel.statusAdv(notifyMessage);
                        
                        messageSent = true;
                    }
                }
                else
                {
                    _peerStatus = PeerStatus.Producer;
                    messageSent = false;
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

        private void ManageStatus(StatusNotifyMessage message)
        {
            if (message.status == PeerStatus.Consumer)
            {
                if (_peerStatus == PeerStatus.Producer)
                {
                    float energyAvailable = getEnergyLevel() - _enThreshold;

                    EnergyProposalMessage propMessage = new EnergyProposalMessage()
                    {
                        header = Tools.getHeader(message.header.Sender, _name),
                        
                        //If peer's energy is >= the request, give the requested energy, otherwise give the en. available
                        energyAvailable = energyAvailable >= message.energyReq ? message.energyReq : energyAvailable,
                        
                        price = 0.9f //TODO: far diventare casuale
                    };

                    Connector.channel.energyProposal(propMessage);
                }
            }
        }

        private void EvaluateProposal(EnergyProposalMessage message)
        {
            if (message.header.Receiver == _name)
                ;
        }
    }
}
