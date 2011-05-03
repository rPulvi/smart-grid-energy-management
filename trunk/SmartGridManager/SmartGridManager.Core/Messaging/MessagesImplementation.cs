using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Core.Commons;

namespace SmartGridManager.Core.Messaging
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Reentrant)]
    public class MessagesImplementation : IMessages
    {
        public virtual void sayHello(GridMessage message)
        {
            Console.WriteLine("Messaggio: {0}\nRicevuto alle: {1}\ninviato da: {2}\ndestinato a: {3}\nTesto: {4}",
                message.header.MessageID,
                message.header.TimeStamp,
                message.header.Sender,
                message.header.Receiver,
                message.descField);          
        }

        public virtual void statusAdv(StatusNotifyMessage message)
        {
            if(message.status == PeerStatus.Consumer)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Ricevuta richiesta di {0}kW di energia da parte di {1}",
                    message.energyReq,                
                    message.header.Sender);
                Console.ResetColor();
            }
        }

        public virtual void energyProposal(EnergyProposalMessage message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Proposta di vendita di {0}kW di energia. Prezzo{1}. Mittente {2}",
                message.energyAvailable,                
                message.price,
                message.header.Sender);
            Console.ResetColor();
        }

        public virtual void acceptProposal(EnergyAcceptMessage message)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Proposta accettata - Energia richiesta {0}",
                message.energy);
            Console.ResetColor();
        }

        public virtual void endProposal(EndProposalMessage message)
        {
            String s;
            if (message.endStatus == true)
            {
                s = "Confermata";
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                s = "Rifiutata";
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.WriteLine("Offerta " + s);
            Console.ResetColor();
        }

        public virtual void heartBeat(HeartBeatMessage message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("heartBeat di {0}, ricevuto alle {1}", message.header.Sender, message.header.TimeStamp);
            Console.ResetColor();
        }

        public virtual void remoteAdv(StatusNotifyMessage message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Remote request sent to Resolver");
            Console.ResetColor();
        }

        public virtual void forwardLocalMessage(PeerMessage message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Peer {0} is forwarding a message..", message.header.Sender);
            Console.ResetColor();
        }
    }
}
