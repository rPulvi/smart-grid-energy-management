using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Linq;
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
        private float _enPeak;        
        private Boolean _state;  // TODO: cambiare nome
        private List<EnergyProposalMessage> _proposalList = new List<EnergyProposalMessage>();
        private System.Timers.Timer _proposalCountdown;

        #endregion

        public PowerManager(String bName, EnergyGenerator generator, float energyPeak)
        {
            this.MsgHandler = Connector.messageHandler;
            
            MsgHandler.OnStatusChanged += new statusNotify(CreateProposal);
            MsgHandler.OnProposalArrived += new energyProposal(ReceiveProposal);

            _generator = generator;
            _enPeak = energyPeak;
            _name = bName;
            _peerStatus = PeerStatus.Producer;
            _state = true;
            _proposalCountdown = new System.Timers.Timer();
            _proposalCountdown.Enabled = false;
            _proposalCountdown.Interval = 5000;
            _proposalCountdown.Elapsed += new ElapsedEventHandler(_proposalCountdown_Elapsed);

        }

        public void Start()
        {
            Boolean messageSent = false;

            peerthread = new Thread(_generator.Start) { IsBackground = true };
            peerthread.Start();

            while (_state == true)
            {
                //Check the energy level
                if (getEnergyLevel() < _enPeak)
                {
                    _peerStatus = PeerStatus.Consumer;

                    if (messageSent == false)
                    {
                        StatusNotifyMessage notifyMessage = new StatusNotifyMessage()
                        {
                            header = Tools.getHeader("All", _name),
                            status = _peerStatus,
                            energyReq = _enPeak - getEnergyLevel()
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

        private void CreateProposal(StatusNotifyMessage message)
        {
            if (message.status == PeerStatus.Consumer)
            {
                if (_peerStatus == PeerStatus.Producer)
                {
                    float enAvailable = getEnergyLevel() - _enPeak;

                    if (enAvailable >= message.energyReq)
                    {
                        EnergyProposalMessage propMessage = new EnergyProposalMessage()
                        {
                            header = Tools.getHeader(message.header.Sender, _name),

                            /* TODO: Optimization required
                            //If peer's energy is >= the request, give the requested energy, otherwise give the en. available
                            energyAvailable = energyAvailable >= message.energyReq ? message.energyReq : energyAvailable,
                            */
                            
                            energyAvailable = message.energyReq,
                            
                            price = 0.9f //TODO: far diventare casuale
                        };

                        Connector.channel.energyProposal(propMessage);
                    }
                }
            }
        }

        private void ReceiveProposal(EnergyProposalMessage message)
        {
            if (message.header.Receiver == _name)
            {
                if(_proposalCountdown.Enabled == false)
                    _proposalCountdown.Enabled = true;
                
                _proposalList.Add(message);                                
            }
        }

        private void _proposalCountdown_Elapsed(object sender, ElapsedEventArgs e)
        {
            _proposalCountdown.Enabled = false;
            EvaluateProposal();
        }

        private void EvaluateProposal()
        {
            var m = (from element in _proposalList
                    orderby element.price ascending
                    select element).First();

            Console.WriteLine("Il prezzo minore è fornito da {0} ed è {1}", m.header.Sender, m.price);                        
        }
    }
}
