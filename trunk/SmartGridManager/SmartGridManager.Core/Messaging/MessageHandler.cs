using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.Commons;

namespace SmartGridManager.Core.Messaging
{
    public delegate void sayHello (String s);
    public delegate void statusNotify (StatusNotifyMessage m);
    public delegate void energyProposal (EnergyProposalMessage m);

    public class MessageHandler : MessagesImplementation
    {
        public event sayHello OnSayHello;
        public event statusNotify OnStatusChanged;
        public event energyProposal OnProposalArrived;

        public override void sayHello(GridMessage message)
        {
            if (OnSayHello != null)
                OnSayHello("Il nodo " + message.header.Sender + " è entrato nella mesh");

            base.sayHello(message);
        }

        public override void statusAdv(StatusNotifyMessage message)
        {
            if (OnStatusChanged != null)
                OnStatusChanged(message);

            base.statusAdv(message);
        }

        public override void energyProposal(EnergyProposalMessage message)
        {
            if (OnProposalArrived != null)
                OnProposalArrived(message);

            base.energyProposal(message);
        }
    }
}
