using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace ConsoleApplication1
{
    public class MessagePassing : IMessagePassing
    {
        public void sendMsg(string message)
        {
            Console.WriteLine(message);        
        }
    }
}
