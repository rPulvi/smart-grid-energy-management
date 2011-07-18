using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Core.Commons;
using SmartGridManager.Core.Utils;

namespace SmartGridManager.Core.Messaging
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Reentrant)]
    public class MessagesImplementation : IMessages
    {
        public virtual void sayHello(HelloMessage message)
        {
            XMLLogger.WriteLocalActivity("Messaggio: " + message.header.MessageID + "Ricevuto alle: " + message.header.TimeStamp +
                " inviato da: " + message.header.Sender + " destinato a: " + message.header.Receiver);
        }

        public virtual void HelloResponse(HelloResponseMessage message)
        { 
            //foo;
        }

        public virtual void statusAdv(StatusNotifyMessage message)
        {
            if(message.status == PeerStatus.Consumer)
                XMLLogger.WriteLocalActivity("Ricevuta richiesta di " + message.energyReq + "kW di energia da parte di " + message.header.Sender);

        }

        public virtual void energyProposal(EnergyProposalMessage message)
        {
            XMLLogger.WriteLocalActivity("Proposta di vendita di "+ message.energyAvailable +"kW di energia. Prezzo " + message.price + " Mittente " + message.header.Sender);
        }

        public virtual void acceptProposal(EnergyAcceptMessage message)
        {
            XMLLogger.WriteLocalActivity("Proposta accettata - Energia richiesta " + message.energy);
        }

        public virtual void endProposal(EndProposalMessage message)
        {
            String s;
            if (message.endStatus == true)            
                s = "Confermata";            
            else            
                s = "Rifiutata";

            XMLLogger.WriteLocalActivity("Offerta " + s);            
        }

        public virtual void heartBeat(HeartBeatMessage message)
        {            
            Console.WriteLine("heartBeat di {0}, ricevuto alle {1}", message.header.Sender, message.header.TimeStamp);
        }

        public virtual void remoteAdv(StatusNotifyMessage message)
        {
            XMLLogger.WriteRemoteActivity("Remote request sent to Resolver");            
        }

        public virtual void forwardEnergyRequest(StatusNotifyMessage message)
        {
            XMLLogger.WriteRemoteActivity("Peer " + message.header.Sender + " is forwarding a message..");
        }

        public virtual void forwardEnergyReply(EndProposalMessage message)
        {
            XMLLogger.WriteRemoteActivity("Energy Request accepted = " + message.endStatus);
        }

        public virtual void updateEnergyStatus(UpdateStatusMessage message)
        {
            XMLLogger.WriteLocalActivity("Updating Stutus");
            XMLLogger.WriteLocalActivity("Peer " + message.header.Sender + " - Energy Sold: " + message.energySold);
            XMLLogger.WriteLocalActivity("Peer " + message.header.Sender + " - Energy Bought: " + message.energyBought);
        }

        public virtual void peerDown(PeerIsDownMessage message)
        {
            XMLLogger.WriteRemoteActivity("Peer " +  message.peerName + " is down");
        }
    }
}
