using System.Collections.Generic;
using System.Linq;
using SmartGridManager.Core.Messaging;
using System.Timers;
using SmartGridManager.Core.Utils;
using SmartGridManager.Core;
using System;

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
        private Guid _originGuid;
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

        public void EnergyLookUp(object m)
        {
            StatusNotifyMessage message = (StatusNotifyMessage)m;

            _originPeerName = message.header.Sender;
            _enLookUp = message.energyReq;
            _originGuid = message.header.MessageID;

            message.header.Sender = this._name;

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
                //EndProposalMessage respMessage = MessageFactory.createEndProposalMessage(
                //    _originGuid,
                //    _name,
                //    "Broker",
                //    false,
                //    0,
                //    0
                //    );

                //Connector.channel.forwardEnergyReply(respMessage);
            }                        
        }

        private void EvaluateProposal()
        {            
            var m = (from element in _proposalList
                    orderby element.price ascending
                    select element).First();            

            EnergyAcceptMessage respMessage = MessageFactory.createEnergyAcceptMessage(
                    m.header.MessageID,
                    m.header.Sender,
                    _name,
                    _originPeerName,
                    _enLookUp);
            
            Connector.channel.acceptProposal(respMessage);

            _proposalList.Clear();
        }

        #endregion
    }
}





