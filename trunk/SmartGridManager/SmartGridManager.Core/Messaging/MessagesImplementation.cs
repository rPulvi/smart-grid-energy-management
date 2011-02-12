using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Core.Commons;

namespace SmartGridManager.Core.Messaging
{

    //[CallbackBehavior(UseSynchronizationContext=false)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Reentrant)]
    public class MessagesImplementation : IMessages
    {
        public virtual  void sayHello(GridMessage message)
        {
            Console.WriteLine("Messaggio: {0}\nRicevuto alle: {1}\ninviato da: {2}\ndestinato a: {3}\nTesto: {4}",
                message.header.MessageID,
                message.header.TimeStamp,
                message.header.Sender,
                message.header.Receiver,
                message.tmpField);          
        }

        public virtual void statusAdv(StatusNotifyMessage message)
        {
            if(message.status == PeerStatus.Consumer)
            {
                Console.WriteLine("Richiesta di {0}kW di energia da parte di {1}",
                    message.energyReq,                
                    message.header.Sender);
            }
        }

        public virtual void energyProposal(EnergyProposalMessage message)
        {
            Console.WriteLine("Proposta di {0}kW di energia. Prezzo{1}. Mittente {2}",
                message.energyAvailable,                
                message.price,
                message.header.Sender);
        }

        public virtual void acceptProposal(EnergyAcceptMessage message)
        {
            Console.WriteLine("Proposta di {0} accettata - Energia richiesta {1}",
                message.header.Receiver,
                message.energy);
        }

        public virtual void endProposal(EndProposalMessage message)
        {
            String s;
            if (message.endStatus == true)
                s="Accettata";
            else
                s="Rifiutata";

            Console.WriteLine("Proposta " + s);
        }
    }
}
