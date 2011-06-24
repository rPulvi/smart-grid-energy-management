using System.Collections.Generic;
using System.Linq;
using SmartGridManager.Core.Messaging;
using System.Timers;
using SmartGridManager.Core.Utils;
using SmartGridManager.Core;

namespace Resolver
{
    class EnergyBroker
    {
        #region Attributes

        private MessageHandler MsgHandler;        
        private string _name;
        private float _enLookUp;

        private string _originPeerName;

        private List<EnergyProposalMessage> _proposalList = new List<EnergyProposalMessage>();
        
        private System.Timers.Timer _proposalCountdown; //Countdown to elaborate the incoming proposal        
        private int _proposalTimeout;

        #endregion

        #region Methods

        public EnergyBroker(string resolverName)
        {
            this.MsgHandler = Connector.messageHandler;
            this._name = resolverName;

            MsgHandler.OnProposalArrived += new energyProposal(ReceiveProposal);            

            _proposalCountdown = new System.Timers.Timer();            
            _proposalCountdown.Interval = 5000;
            _proposalCountdown.Elapsed += new ElapsedEventHandler(_proposalCountdown_Elapsed);
            _proposalCountdown.Enabled = false;           
        }

        public void EnergyLookUp(StatusNotifyMessage message)
        {
            _enLookUp = message.energyReq;

            XMLLogger.WriteRemoteActivity("Broker is serving: " + message.header.MessageID + " session.");

            Connector.channel.statusAdv(message);

            _proposalCountdown.Enabled = true;
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
                //_proposalTimeout++;

                //if (_proposalTimeout > 2)  //Go Outbound
                //{
                //    float enReq = _enPeak - (getEnergyLevel() + _enBought);
                //    Connector.channel.forwardLocalMessage(MessageFactory.createEnergyRequestMessage(_resolverName, _name, _peerStatus, enReq));
                //    messageSent = true;

                //    _proposalTimeout = 0;
                    
                //    //start the timer to waiting for proposals
                //    if (_proposalCountdown.Enabled == false)
                //        _proposalCountdown.Enabled = true;
                //}
                //else
                //{
                //    XMLLogger.WriteLocalActivity("Nessuna offerta energetica ricevuta");                    
                //    messageSent = false; //send the request message again                
                //}
            }
        }

        private void EvaluateProposal()
        {            
            var m = (from element in _proposalList
                    orderby element.price ascending
                    select element).First();

            XMLLogger.WriteLocalActivity("Il prezzo minore è fornito da " + m.header.Sender + " ed è " + m.price);            

            EnergyAcceptMessage respMessage = MessageFactory.createEnergyAcceptMessage(
                    m.header.MessageID,
                    m.header.Sender,
                    _name,
                    _enLookUp);
            
            Connector.channel.acceptProposal(respMessage);
        }

        #endregion
    }
}





