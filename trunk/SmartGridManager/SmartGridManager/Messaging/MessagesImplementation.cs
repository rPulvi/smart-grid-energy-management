using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Messaging;

namespace SmartGridManager.Messaging
{
    class MessagesImplementation : IMessages
    {        
        public void sayHello(GridMessage message)
        {
            Console.WriteLine("Messaggio: {0}\nRicevuto alle: {1}\ninviato da: {2}\ndestinato a: {3}\nTesto: {4}",
                message.header.MessageID,
                message.header.TimeStamp,
                message.header.Sender,
                message.header.Receiver,
                message.tmpField);
        }

        public void statusAdv(AdvertisingMessage message)
        {
            Console.WriteLine("Messaggio: {0}\nRicevuto alle: {1}\ninviato da: {2}\nStato:{3}",
                message.header.MessageID,
                message.header.TimeStamp,
                message.header.Sender,
                message.status);
        }
    }
}
