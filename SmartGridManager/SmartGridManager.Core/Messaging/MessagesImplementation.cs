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
        public virtual void sayHello(HelloMessage message)
        {
            Console.WriteLine("Messaggio: {0}\nRicevuto alle: {1}\ninviato da: {2}\ndestinato a: {3}\n",
                message.header.MessageID,
                message.header.TimeStamp,
                message.header.Sender,
                message.header.Receiver);          
        }

        public virtual void HelloResponse(HelloResponseMessage message)
        { 
            //foo;
        }

        public virtual void statusAdv(StatusNotifyMessage message)
        {
            if(message.status == PeerStatus.Consumer)
            {
                Console.WriteLine("Ricevuta richiesta di {0}kW di energia da parte di {1}",
                    message.energyReq,                
                    message.header.Sender);                
            }
        }

        public virtual void energyProposal(EnergyProposalMessage message)
        {            
            Console.WriteLine("Proposta di vendita di {0}kW di energia. Prezzo{1}. Mittente {2}",
                message.energyAvailable,                
                message.price,
                message.header.Sender);         
        }

        public virtual void acceptProposal(EnergyAcceptMessage message)
        {
            Console.WriteLine("Proposta accettata - Energia richiesta {0}",
                message.energy);
        }

        public virtual void endProposal(EndProposalMessage message)
        {
            String s;
            if (message.endStatus == true)            
                s = "Confermata";                
            
            else            
                s = "Rifiutata";

            Console.WriteLine("Offerta " + s);
        }

        public virtual void heartBeat(HeartBeatMessage message)
        {
            Console.WriteLine("heartBeat di {0}, ricevuto alle {1}", message.header.Sender, message.header.TimeStamp);
        }

        public virtual void remoteAdv(StatusNotifyMessage message)
        {
            Console.WriteLine("Remote request sent to Resolver");         
        }

        public virtual void forwardLocalMessage(PeerMessage message)
        {
            Console.WriteLine("Peer {0} is forwarding a message..", message.header.Sender);
        }

        public virtual void updateEnergyStatus(UpdateStatusMessage message)
        {
            Console.WriteLine("Peer {0} - Energy Sold: {1}", message.header.Sender, message.energySold);
            Console.WriteLine("Peer {0} - Energy Bought: {1}", message.header.Sender, message.energyBought);
        }

        public virtual void peerDown(PeerIsDownMessage message)
        {
            Console.WriteLine("Peer {0} is down...", message.peerName);
        }
    }
}
