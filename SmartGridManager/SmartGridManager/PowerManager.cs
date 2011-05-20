using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        
        private String _name;
        private string _resolverName; ///
        private PeerStatus _peerStatus;
        private float _enPeak;
        private float _price;
        private float _enBought;
        private float _enSold;
        private int _proposalTimeout = 0;
        private Boolean _loop;
        private Boolean messageSent = false;
        private List<EnergyProposalMessage> _proposalList = new List<EnergyProposalMessage>();
        private Dictionary<System.Timers.Timer, EnergyLink> _producers = new Dictionary<System.Timers.Timer, EnergyLink>();
        private Dictionary<System.Timers.Timer, EnergyLink> _consumers = new Dictionary<System.Timers.Timer, EnergyLink>();
        private System.Timers.Timer _proposalCountdown;
        private System.Timers.Timer _heartBeatTimer;

        #endregion

        #region Methods

        public PowerManager(String bName, EnergyGenerator generator, float energyPeak, float price)
        {
            this.MsgHandler = Connector.messageHandler;
            
            MsgHandler.OnHelloResponse += new HelloResponse(ReceiveResolverName);
            MsgHandler.OnStatusChanged += new statusNotify(CreateProposal);
            MsgHandler.OnProposalArrived += new energyProposal(ReceiveProposal);
            MsgHandler.OnProposalAccepted += new acceptProposal(ProposalAccepted);
            MsgHandler.OnEndProposalArrived += new endProposal(EndProposal);
            MsgHandler.OnHeartBeat += new heartBeat(CheckHeartBeat);

            _generator = generator;
            _enPeak = energyPeak;
            _name = bName;
            
            _loop = true;
            _price = price;
            _enBought = 0f;
            _enSold = 0f;

            _proposalCountdown = new System.Timers.Timer();            
            _proposalCountdown.Interval = 5000;
            _proposalCountdown.Elapsed += new ElapsedEventHandler(_proposalCountdown_Elapsed);
            _proposalCountdown.Enabled = false;

            _heartBeatTimer = new System.Timers.Timer();
            _heartBeatTimer.Enabled = true;
            _heartBeatTimer.Interval = 2000;
            _heartBeatTimer.Elapsed += new ElapsedEventHandler(_heartBeatTimer_Elapsed);
            _heartBeatTimer.Start();
        }

        public void Start()
        {            
            while (_loop == true)
            {
                //Check the energy level
                if ((getEnergyLevel() + _enBought) < _enPeak)
                {
                    //became Consumer
                    _peerStatus = PeerStatus.Consumer;

                    if (messageSent == false)
                    {
                        float enReq = _enPeak - getEnergyLevel() + _enBought;
                        Connector.channel.statusAdv(MessageFactory.createEnergyRequestMessage("@All", _name, _peerStatus, enReq));

                        //start the timer to waiting for proposals
                        if (_proposalCountdown.Enabled == false)
                            _proposalCountdown.Enabled = true;
                        
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

        private void ReceiveResolverName(HelloResponseMessage message)
        {            
            _resolverName = message.ResolverName;
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
                        EnergyProposalMessage respMessage = MessageFactory.createEnergyProposalMessage(
                            message.header.Sender,
                            _name,
                            message.energyReq,
                            _price);

                        if (message.header.Sender == _resolverName)
                            Connector.channel.forwardLocalMessage(respMessage);
                        else
                            Connector.channel.energyProposal(respMessage);
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
            {
                _proposalTimeout = 0;
                EvaluateProposal();
            }
            else
            {
                Console.WriteLine("Nessuna offerta energetica ricevuta");
                messageSent = false; //send the request message again
                _proposalTimeout++;

                if (_proposalTimeout == 3)  //Go Outbound
                {
                    messageSent = true;
                    Connector.channel.forwardLocalMessage(MessageFactory.createEnergyRequestMessage(_resolverName, _name, _peerStatus, _enPeak - getEnergyLevel() + _enBought));
                    _proposalTimeout = 0;
                }
            }
        }

        private void EvaluateProposal()
        {            
            var m = (from element in _proposalList
                    orderby element.price ascending
                    select element).First();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Il prezzo minore è fornito da {0} ed è {1}", m.header.Sender, m.price);
            Console.ResetColor();

            EnergyAcceptMessage respMessage = MessageFactory.createEnergyAcceptMessage(
                    m.header.Sender,
                    _name,
                    _enPeak - getEnergyLevel() + _enBought);
            
            if (m.header.Sender == _resolverName)
                Connector.channel.forwardLocalMessage(respMessage);
            else
                Connector.channel.acceptProposal(respMessage);

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
                    _enSold += message.energy;

                    System.Timers.Timer t = getLinkTimer();
                    EnergyLink link = new EnergyLink(message.header.Sender, message.energy);
                    
                    _consumers.Add(t, link);

                    t.Enabled = false;
                }

                EndProposalMessage respMessage = MessageFactory.createEndProposalMessage(
                    message.header.Sender,
                    _name,
                    status,
                    message.energy);

                if (message.header.Sender == _resolverName)
                    Connector.channel.forwardLocalMessage(respMessage);
                else
                    Connector.channel.endProposal(respMessage);
            }
        }

        private void EndProposal(EndProposalMessage message)
        {
            if(message.header.Receiver == _name)
            {
                if (message.endStatus == true)
                {
                    _enBought += message.energy;

                    System.Timers.Timer t = getLinkTimer();
                    EnergyLink link = new EnergyLink(message.header.Sender, message.energy);

                    _producers.Add(t, link);

                    t.Enabled = false;
                }
            }
        }

        private void heartBeatTimeout(object sender, ElapsedEventArgs e)
        {
            if (_producers.ContainsKey((System.Timers.Timer)sender))
            {
                foreach (var p in _producers)
                {
                    //TODO: rimuovere timer dalla memoria!!! .... ???

                    if (p.Key == sender)
                    {
                        p.Key.Enabled = false;
                        _enBought -= p.Value.energy;
                        break;
                    }
                }

                _producers.Remove((System.Timers.Timer)sender);
            }
            else if (_consumers.ContainsKey((System.Timers.Timer)sender))
            {
                foreach (var c in _consumers)
                {
                    //TODO: rimuovere timer dalla memoria!!!

                    if (c.Key == sender)
                    {
                        c.Key.Enabled = false;
                        _enSold -= c.Value.energy;
                        break;
                    }
                }

                _consumers.Remove((System.Timers.Timer)sender);
            }
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
                if (p.Value.peerName == message.header.Sender)
                {
                    //stop & restart the timer
                    p.Key.Enabled = false;
                    p.Key.Enabled = true;
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
                if (c.Value.peerName == message.header.Sender)
                {
                    c.Key.Enabled = false;
                    c.Key.Enabled = true;
                    break;
                }
            }
        }

        private void _heartBeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // ???
            Connector.channel.heartBeat(MessageFactory.createHeartBeatMessage(_name));
        }

        public float getEnergyLevel() { return _generator.EnergyLevel; }

        // TODO: Spostare il metodo da un'altra parte
        public void setEnergyLevel(float value)
        {
            _generator.EnergyLevel = value;
        }

        public void ShutDown()
        {
            _generator.Stop();
            _loop = false;
        }

        private System.Timers.Timer getLinkTimer()
        {
            System.Timers.Timer t = new System.Timers.Timer();
            t.Enabled = true;
            t.Interval = 10000;
            t.Elapsed += new ElapsedEventHandler(heartBeatTimeout);

            return t;
        }

        #endregion

        private class EnergyLink
        {
            public string peerName { get; private set; }
            public float energy { get; private set; }

            public EnergyLink(string name, float en)
            {
                this.peerName = name;
                this.energy = en;
            }
        }
    }
}

