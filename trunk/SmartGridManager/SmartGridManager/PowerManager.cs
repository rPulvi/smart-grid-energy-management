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
        #region CONSTs HERE
        /********************************/
        private const int TTL = 4;
        /********************************/
        #endregion

        #region Attributes

        private EnergyGenerator _generator;        
        private MessageHandler MsgHandler;
        
        private string _name;
        private string _resolverName;

        private int _proposalTimeout = 0;
        private float _enPeak;
        private float _price;
        private float _enBought;
        private float _enSold;               
        
        private Boolean messageSent = false;
        
        private PeerStatus _peerStatus;
        private List<EnergyProposalMessage> _proposalList = new List<EnergyProposalMessage>();

        public ObservableCollectionEx<EnergyLink> producers { get; private set; }
        public ObservableCollectionEx<EnergyLink> consumers { get; private set; }
        
        #region Timers
        private System.Timers.Timer _proposalCountdown; //Countdown to elaborate the incoming proposal
        private System.Timers.Timer _heartBeatTimer; //Heartbeat frequency 
        private System.Timers.Timer _mainTimer; //The main cycle
        #endregion

        #endregion

        #region Methods

        public PowerManager(String bName, PeerStatus status,  EnergyGenerator generator, float energyPeak, float price)
        {
            this.MsgHandler = Connector.messageHandler;

            #region EventListeners
            MsgHandler.OnHelloResponse += new HelloResponse(ReceiveResolverName);
            MsgHandler.OnStatusChanged += new statusNotify(CreateProposal);
            MsgHandler.OnProposalArrived += new energyProposal(ReceiveProposal);
            MsgHandler.OnProposalAccepted += new acceptProposal(ProposalAccepted);
            MsgHandler.OnEndProposalArrived += new endProposal(EndProposal);            
            MsgHandler.OnPeerDown += new alertPeerDown(SomePeerIsDown);
            #endregion

            #region Init Session                        
            
            _generator = generator;
            _enPeak = energyPeak;
            _name = bName;
            _peerStatus = status;

            producers = new ObservableCollectionEx<EnergyLink>();
            consumers = new ObservableCollectionEx<EnergyLink>();

            _price = price;
            _enBought = 0f;
            _enSold = 0f;

            #region Timing Zone
            _proposalCountdown = new System.Timers.Timer();            
            _proposalCountdown.Interval = 5000;
            _proposalCountdown.Elapsed += new ElapsedEventHandler(_proposalCountdown_Elapsed);
            _proposalCountdown.Enabled = false;

            _heartBeatTimer = new System.Timers.Timer();
            _heartBeatTimer.Enabled = false;
            _heartBeatTimer.Interval = 2000;
            _heartBeatTimer.Elapsed += new ElapsedEventHandler(_heartBeatTimer_Elapsed);            

            _mainTimer = new System.Timers.Timer();
            _mainTimer.Interval = 500;
            _mainTimer.Elapsed += new ElapsedEventHandler(_mainTimer_Elapsed);
            _mainTimer.Enabled = false;            
            #endregion
            
            #endregion
        }

        public void Start()
        {
            _mainTimer.Enabled = true;
            _heartBeatTimer.Enabled = true;
        }

        /// <summary>
        /// This Event manages the main Application Lifecycle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Check the energy level
            if ((getEnergyLevel() + _enBought) < _enPeak)
            {
                if (messageSent == false)
                {
                    float enReq = _enPeak - (getEnergyLevel() + _enBought);
                    Connector.channel.statusAdv(MessageFactory.createEnergyRequestMessage("@All", _name, _peerStatus, enReq));

                    messageSent = true;

                    //start the timer to waiting for proposals
                    if (_proposalCountdown.Enabled == false)
                        _proposalCountdown.Enabled = true;
                }
            }
            else
            {
                messageSent = false;
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
                    float enAvailable = getEnergyLevel() - (_enPeak - _enSold);

                    if (enAvailable >= 0)
                    {
                        float enSold = enAvailable >= message.energyReq ? message.energyReq : enAvailable;

                        EnergyProposalMessage respMessage = MessageFactory.createEnergyProposalMessage(
                            message.header.MessageID,
                            message.header.Sender,
                            _name,
                            enSold,
                            _price
                            );
                        
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
                _proposalTimeout++;

                if (_proposalTimeout > 2)  //Go Outbound
                {
                    float enReq = _enPeak - (getEnergyLevel() + _enBought);

                    XMLLogger.WriteRemoteActivity("Going Outside to find " + enReq + " KW/h");

                    Connector.channel.forwardEnergyRequest(MessageFactory.createEnergyRequestMessage(_resolverName, _name, _peerStatus, enReq));
                    messageSent = true;

                    _proposalTimeout = 0;

                    //start the timer to waiting for proposals
                    if (_proposalCountdown.Enabled == false)
                        _proposalCountdown.Enabled = true;
                }
                else
                {
                    XMLLogger.WriteLocalActivity("Nessuna offerta energetica ricevuta");                    
                    messageSent = false; //send the request message again                
                }
            }
        }

        private void EvaluateProposal()
        {            
            var m = (from element in _proposalList
                    orderby element.price ascending
                    select element).First();

            XMLLogger.WriteLocalActivity("The lowest price is " + m.price + " provided by " + m.header.Sender);

            EnergyAcceptMessage respMessage = MessageFactory.createEnergyAcceptMessage(
                    m.header.MessageID,
                    m.header.Sender,
                    _name,
                    this._name,
                    _enPeak - (getEnergyLevel() + _enBought));
            
            Connector.channel.acceptProposal(respMessage);
        }

        private void ProposalAccepted(EnergyAcceptMessage message)
        {           
            if (message.header.Receiver == _name)
            {
                Boolean status = false;

                if (message.energy <= (getEnergyLevel() - (_enPeak - _enSold)))
                {
                    status = true;
                    _enSold += message.energy;

                    XMLLogger.WriteLocalActivity("Ok, " + message.energy + " KW/h sold to " + message.header.Sender);                    

                    EnergyLink link = new EnergyLink(message.peerName, message.energy,_price);
                    consumers.Add(link);

                    //Advise the Local Resolver About the energy status change.                    
                    Connector.channel.updateEnergyStatus(MessageFactory.createUpdateStatusMessage(_resolverName,_name,_enSold,_enBought));
                }

                EndProposalMessage respMessage = MessageFactory.createEndProposalMessage(
                    message.header.MessageID,
                    message.header.Sender,
                    _name,
                    status,
                    message.energy,
                    _price
                    );

                if (message.header.Sender == _resolverName)
                    Connector.channel.forwardEnergyReply(respMessage);
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

                    XMLLogger.WriteLocalActivity("Energy received from " + message.header.Sender);                    

                    EnergyLink link = new EnergyLink(message.header.Sender, message.energy,message.price);
                    producers.Add(link);

                    //Advise the Local Resolver About the energy status change.
                    Connector.channel.updateEnergyStatus(MessageFactory.createUpdateStatusMessage(_resolverName, _name, _enSold, _enBought));

                    _proposalList.Clear();
                }
                else
                {
                    //Nothing to do..Delete this Producer and go on with the next.
                    for (int i = 0; i < _proposalList.Count; i++)
                    {
                        if (_proposalList[i].header.Sender == message.header.Sender)
                        {
                            _proposalList.RemoveAt(i);
                            break;
                        }
                    }

                    //More Proposal? Evaluate it or Start again
                    if (_proposalList.Count > 0)
                        EvaluateProposal();
                    else
                        messageSent = false;
                }
            }
        }

        private void _heartBeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {            
            Connector.channel.heartBeat(MessageFactory.createHeartBeatMessage(_name));
        }

        private void SomePeerIsDown(PeerIsDownMessage message)
        { 
            //Update my Energy Consumers List
            for (int i = 0; i < consumers.Count; i++)
            {
                if (consumers[i].peerName == message.peerName)
                {
                    _enSold = _enSold - consumers[i].energy;
                    consumers.RemoveAt(i);
                    Connector.channel.updateEnergyStatus(MessageFactory.createUpdateStatusMessage(_resolverName, _name, _enSold, _enBought));
                    break;
                }
            }

            //Update my Energy Producers (Suppliers) List
            for (int i = 0; i < producers.Count; i++)
            {
                if (producers[i].peerName == message.peerName)
                {
                    _enBought = _enBought - producers[i].energy;
                    producers.RemoveAt(i);
                    Connector.channel.updateEnergyStatus(MessageFactory.createUpdateStatusMessage(_resolverName, _name, _enSold, _enBought));
                    break;
                }
            }
        }

        public float getEnergyLevel() { return _generator.EnergyLevel; }
        
        public void setEnergyLevel(float value) { _generator.EnergyLevel = value; }

        public void ShutDown()
        {
            _proposalCountdown.Enabled = false;
            _heartBeatTimer.Enabled = false;
            _mainTimer.Enabled = false;
            _generator.Stop();            
        }

        #endregion
    }
}

