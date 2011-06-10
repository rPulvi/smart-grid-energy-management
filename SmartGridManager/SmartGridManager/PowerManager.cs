using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using SmartGridManager.Core;
using SmartGridManager.Core.Messaging;
using SmartGridManager.Core.Commons;
using SmartGridManager.Core.Utils;
using System.Threading;

namespace SmartGridManager
{
    class PowerManager
    {

        #region Attributes

        private EnergyGenerator _generator;        
        private MessageHandler MsgHandler;

        private const int TTL = 4;
        private String _name;
        private string _resolverName; ///
        private PeerStatus _peerStatus;
        private float _enPeak;
        private float _price;
        private float _enBought;
        private float _enSold;
        private int _proposalTimeout = 0;
        
        private Boolean messageSent = false;
        private List<EnergyProposalMessage> _proposalList = new List<EnergyProposalMessage>();
        private List<EnergyLink> _producers = new List<EnergyLink>();
        private List<EnergyLink> _consumers = new List<EnergyLink>();
        private System.Timers.Timer _proposalCountdown;
        private System.Timers.Timer _heartBeatTimer;
        private System.Timers.Timer _mainTimer;

        #endregion

        #region Methods

        public PowerManager(String bName, PeerStatus status,  EnergyGenerator generator, float energyPeak, float price)
        {
            this.MsgHandler = Connector.messageHandler;
            
            MsgHandler.OnHelloResponse += new HelloResponse(ReceiveResolverName);
            MsgHandler.OnStatusChanged += new statusNotify(CreateProposal);
            MsgHandler.OnProposalArrived += new energyProposal(ReceiveProposal);
            MsgHandler.OnProposalAccepted += new acceptProposal(ProposalAccepted);
            MsgHandler.OnEndProposalArrived += new endProposal(EndProposal);            
            MsgHandler.OnPeerDown += new alertPeerDown(LookForPeerDown);

            _generator = generator;
            _enPeak = energyPeak;
            _name = bName;
            _peerStatus = status;
                       
            _price = price;
            _enBought = 0f;
            _enSold = 0f;

            _proposalCountdown = new System.Timers.Timer();            
            _proposalCountdown.Interval = 5000;
            _proposalCountdown.Elapsed += new ElapsedEventHandler(_proposalCountdown_Elapsed);
            _proposalCountdown.Enabled = false;

            _heartBeatTimer = new System.Timers.Timer();
            _heartBeatTimer.Enabled = true;
            _heartBeatTimer.Interval = 3000;
            _heartBeatTimer.Elapsed += new ElapsedEventHandler(_heartBeatTimer_Elapsed);
            _heartBeatTimer.Start();

            _mainTimer = new System.Timers.Timer();
            _mainTimer.Interval = 500;
            _mainTimer.Elapsed += new ElapsedEventHandler(_mainTimer_Elapsed);
            _mainTimer.Enabled = false;
        }

        void _mainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Check the energy level
            if ((getEnergyLevel() + _enBought) < _enPeak)
            {
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
                messageSent = false;
            }
        }

        public void Start()
        {
            _mainTimer.Enabled = true;
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
                    
                    EnergyLink link = new EnergyLink(message.header.Sender, message.energy);

                    _consumers.Add(link);

                    Connector.channel.updateEnergyStatus(MessageFactory.createUpdateStatusMessage(_resolverName,_name,_enSold,_enBought));
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

                    EnergyLink link = new EnergyLink(message.header.Sender, message.energy);

                    _producers.Add(link);

                    Connector.channel.updateEnergyStatus(MessageFactory.createUpdateStatusMessage(_resolverName,_name,_enSold,_enBought));
                }
            }
        }

        private void _heartBeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // ???
            Connector.channel.heartBeat(MessageFactory.createHeartBeatMessage(_name));
        }

        private void LookForPeerDown(PeerIsDownMessage message)
        { 
            //Update my Energy Consumers List
            for (int i = 0; i < _consumers.Count; i++)
            {
                if (_consumers[i].peerName == message.peerName)
                {
                    _enSold = _enSold - _consumers[i].energy;
                    _consumers.RemoveAt(i);
                    Connector.channel.updateEnergyStatus(MessageFactory.createUpdateStatusMessage(_resolverName, _name, _enSold, _enBought));
                    break;
                }
            }

            //Update my Energy Producers (Suppliers) List
            for (int i = 0; i < _producers.Count; i++)
            {
                if (_producers[i].peerName == message.peerName)
                {
                    _enBought = _enBought - _producers[i].energy;
                    _producers.RemoveAt(i);
                    Connector.channel.updateEnergyStatus(MessageFactory.createUpdateStatusMessage(_resolverName, _name, _enSold, _enBought));
                    break;
                }
            }
        }

        public float getEnergyLevel() { return _generator.EnergyLevel; }

        // TODO: Spostare il metodo da un'altra parte
        public void setEnergyLevel(float value) { _generator.EnergyLevel = value; }

        public void ShutDown()
        {
            _proposalCountdown.Enabled = false;
            _heartBeatTimer.Enabled = false;
            _mainTimer.Enabled = false;
            _generator.Stop();            
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

