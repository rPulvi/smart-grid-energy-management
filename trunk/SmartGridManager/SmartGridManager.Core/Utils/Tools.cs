using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.Messaging;

namespace SmartGridManager.Core.Utils
{
    public static class Tools
    {
        public static StandardMessageHeader getHeader(String receiver, String sender )
        { 
            StandardMessageHeader m;

            m = new StandardMessageHeader{ 
                MessageID = Guid.NewGuid(), 
                Receiver = receiver, 
                Sender = sender, 
                TimeStamp = DateTime.Now };

            return m;
        }
    }
}
