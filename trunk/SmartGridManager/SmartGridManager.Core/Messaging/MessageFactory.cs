using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartGridManager.Core.Utils;
using SmartGridManager.Core.Commons;


namespace SmartGridManager.Core.Messaging
{
    public static class MessageFactory
    {
        public static StatusNotifyMessage createEnergyRequestMessage(string name, PeerStatus peerStatus, float enReq)
        {
            StatusNotifyMessage notifyMessage = new StatusNotifyMessage()
            {
                header = Tools.getHeader("@All", name),
                status = peerStatus,
                energyReq = enReq
            };

            return notifyMessage;
        }
    }
}
