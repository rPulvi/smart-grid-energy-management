using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.Messaging;

namespace SmartGridManager.Core.Utils
{
    public static class Tools
    {
        /// <summary>
        /// return a StandardMessageHeader generating a new message ID and applying the TimeStamp
        /// </summary>
        /// <param name="receiver">message receiver</param>
        /// <param name="sender">message sender</param>
        /// <returns>the message header</returns>
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
