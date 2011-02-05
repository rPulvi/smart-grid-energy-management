using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartGridManager.Messaging
{
    public delegate void sayHello (String s);
    public delegate void statusNotify (PeerStatus s);

    public class MessageHandler : MessagesImplementation
    {
        public event sayHello OnSayHello;
        public event statusNotify OnStatusChanged;

        public override void sayHello(GridMessage message)
        {
            if (OnSayHello != null)
                OnSayHello("ciao");

            base.sayHello(message);
        }

        public override void statusAdv(StatusNotifyMessage message)
        {
            if (OnStatusChanged != null)
                OnStatusChanged(message.status);

            base.statusAdv(message);
        }
    }
}
