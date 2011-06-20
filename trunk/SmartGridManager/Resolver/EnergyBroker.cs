using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.Messaging;
using SmartGridManager.Core.Commons;
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

        public EnergyBroker(MessageHandler msgHandler, string resolverName)
        {
            this.MsgHandler = msgHandler;
            this._name = resolverName;

            #region EventListeners
            MsgHandler.OnProposalArrived += new energyProposal(ReceiveProposal);
            MsgHandler.OnEndProposalArrived += new endProposal(EndProposal);
            #endregion

            _proposalCountdown = new System.Timers.Timer();            
            _proposalCountdown.Interval = 5000;
            _proposalCountdown.Elapsed += new ElapsedEventHandler(_proposalCountdown_Elapsed);
            _proposalCountdown.Enabled = false;           
        }

        public void EnergyLookUp(StatusNotifyMessage message)
        {
            _originPeerName = message.header.Sender;
            message.header.Sender = this._name;
            _enLookUp = message.energyReq;

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
                    m.header.Sender,
                    _name,
                    _enLookUp);
            
            Connector.channel.acceptProposal(respMessage);
        }

        private void EndProposal(EndProposalMessage message)
        {
            if (message.header.Receiver == this._name)
            {
                message.header.Receiver = _originPeerName;
                Connector.channel.forwardEnergyReply(message);
            }
        }

        #endregion
    }
}





