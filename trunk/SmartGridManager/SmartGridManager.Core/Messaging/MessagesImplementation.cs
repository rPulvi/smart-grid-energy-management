using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;


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
            Console.WriteLine("Messaggio: {0}\nRicevuto alle: {1}\ninviato da: {2}\nStato:{3}",
                message.header.MessageID,
                message.header.TimeStamp,
                message.header.Sender,
                (int)message.status);           
        }
    }
}
