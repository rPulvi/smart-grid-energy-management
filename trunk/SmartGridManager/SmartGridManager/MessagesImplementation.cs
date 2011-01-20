using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SmartGridManager
{
    public interface IChannel : IMessages, IClientChannel
    {
    }

    class MessagesImplementation : IMessages
    {
        public void sayHello(myMessage message)
        {
            Console.WriteLine("Messaggio Ricevuto: {0}", message.Name );
        }
    }
}
