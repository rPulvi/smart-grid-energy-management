using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SmartGridManager
{
    [MessageContract]
    public class myMessage
    {
        private string name;

        //Constructor - create an empty message.

        public myMessage() { }

        //Constructor - create a message and populate its members.

        public myMessage(string name)
        {
            this.name = name;
        }

        //Constructor - create a message from another message.

        public myMessage(myMessage message)
        {
            this.name = message.name;
        }

        [MessageHeader]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

    }

}
