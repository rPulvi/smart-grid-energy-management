using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
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
        private float _price;
        private float _enBought;
        private float _enSold;
        private Boolean _loop;
        private List<EnergyProposalMessage> _proposalList = new List<EnergyProposalMessage>();
        private Dictionary<System.Timers.Timer, String> _producers = new Dictionary<System.Timers.Timer, String>();
        private Dictionary<System.Timers.Timer, String> _consumers = new Dictionary<System.Timers.Timer, String>();
        private System.Timers.Timer _proposalCountdown;
        private System.Timers.Timer _heartBeatTimer;

        #endregion

        public PowerManager(String bName, EnergyGenerator generator, float energyPeak, float price)
        {
            this.MsgHandler = Connector.messageHandler;
            
            MsgHandler.OnStatusChanged += new statusNotify(CreateProposal);
            MsgHandler.OnProposalArrived += new energyProposal(ReceiveProposal);
            MsgHandler.OnProposalAccepted += new acceptProposal(ProposalAccepted);
            MsgHandler.OnEndProposalArrived += new endProposal(EndProposal);
            MsgHandler.OnHeartBeat += new heartBeat(CheckHeartBeat);

            _generator = generator;
            _enPeak = energyPeak;
            _name = bName;
            _peerStatus = PeerStatus.Producer;
            _loop = true;
            
            _proposalCountdown = new System.Timers.Timer();
            _proposalCountdown.Enabled = false;
            _proposalCountdown.Interval = 5000;
            _proposalCountdown.Elapsed += new ElapsedEventHandler(_proposalCountdown_Elapsed);

            _heartBeatTimer = new System.Timers.Timer();
            _heartBeatTimer.Enabled = true;
            _heartBeatTimer.Interval = 2000;
            _heartBeatTimer.Elapsed += new ElapsedEventHandler(_heartBeatTimer_Elapsed);
            _heartBeatTimer.Start();

            _price = price;
            _enBought = 0f;
            _enSold = 0f;
        }

        public void Start()
        {
            Boolean messageSent = false;

            peerthread = new Thread(_generator.Start) { IsBackground = true };
            peerthread.Start();

            while (_loop == true)
            {
                //Check the energy level
                if ((getEnergyLevel() + _enBought) < _enPeak)
                {
                    //became Consumer
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
                        
                        //start timer
                        if (_proposalCountdown.Enabled == false)
                            _proposalCountdown.Enabled = true;
                        
                        messageSent = true;
                    }
                }
                else
                {
                    //became Producer
                    _peerStatus = PeerStatus.Producer;
                    messageSent = false;
                }
            }
        }

        public void ShutDown()
        {            
            _generator.Stop();
            _loop = false;
        }

        public float getEnergyLevel() { return _generator.EnergyLevel; }
        
        // TODO: Spostare il metodo da un'altra parte
        public void setEnergyLevel(float value)
        {
            _generator.level = value;
        }

        private void CreateProposal(StatusNotifyMessage message)
        {
            if (message.status == PeerStatus.Consumer)
            {
                if (_peerStatus == PeerStatus.Producer)
                {
                    float enAvailable = getEnergyLevel() - _enPeak - _enSold;

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
                            
                            price = _price
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
                _proposalList.Add(message);                                
            }
        }

        private void _proposalCountdown_Elapsed(object sender, ElapsedEventArgs e)
        {
            _proposalCountdown.Enabled = false; //Stop the timer
            
            if (_proposalList.Count > 0)
                EvaluateProposal();
            else
                Console.WriteLine("Nessuna offerta energetica ricevuta");
        }

        private void EvaluateProposal()
        {            
            var m = (from element in _proposalList
                    orderby element.price ascending
                    select element).First();

            Console.WriteLine("Il prezzo minore è fornito da {0} ed è {1}", m.header.Sender, m.price);
            
            EnergyAcceptMessage acceptMessage = new EnergyAcceptMessage()
            {
                header = Tools.getHeader(m.header.Sender,_name),
                energy = _enPeak - getEnergyLevel()
            };

            Connector.channel.acceptProposal(acceptMessage);

            _proposalList.Clear();
        }

        private void ProposalAccepted(EnergyAcceptMessage message)
        {           
            if (message.header.Receiver == _name)
            {
                Boolean status = false;

                if (message.energy <= (getEnergyLevel() - _enPeak - _enSold))
                {
                    status = true;
                    _enSold = message.energy;

                    System.Timers.Timer t = new System.Timers.Timer();
                    t.Enabled = true;
                    t.Interval = 10000;
                    t.Elapsed += new ElapsedEventHandler(heartBeatTimeout);

                    _consumers.Add(t, message.header.Sender);
                }

                EndProposalMessage endMessage = new EndProposalMessage()
                {
                    header = Tools.getHeader(message.header.Sender, _name),
                    endStatus = status,
                    energy = message.energy
                };

                Connector.channel.endProposal(endMessage);
            }
        }

        private void EndProposal(EndProposalMessage message)
        {
            if(message.header.Receiver == _name)
            {
                if (message.endStatus == true)
                {
                    _enBought = message.energy;

                    System.Timers.Timer t = new System.Timers.Timer();
                    t.Enabled = true;
                    t.Interval = 10000;
                    t.Elapsed += new ElapsedEventHandler(heartBeatTimeout);

                    _producers.Add(t, message.header.Sender);
                }
            }
        }

        private void _heartBeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            HeartBeatMessage message = new HeartBeatMessage()
            {
                header = Tools.getHeader("@all", _name)
            };

            Connector.channel.heartBeat(message);
        }

        private void heartBeatTimeout(object sender, ElapsedEventArgs e)
        {
            _producers.Remove((System.Timers.Timer)sender);
        }

        private void CheckHeartBeat(HeartBeatMessage message)
        {
            CheckHBProducers(message);
            CheckHBConsumers(message);
        }

        private void CheckHBProducers(HeartBeatMessage message)
        {
            foreach (var p in _producers)
            {

                //value = producer
                //key = timer of this producer
                if (p.Value == message.header.Sender)
                {
                    p.Key.Start();
                    break;
                }
            }
        }

        private void CheckHBConsumers(HeartBeatMessage message)
        {
            foreach (var c in _consumers)
            {

                //value = consumer
                //key = timer of this consumer
                if (c.Value == message.header.Sender)
                {
                    c.Key.Start();
                    break;
                }
            }
        }

    }
}
