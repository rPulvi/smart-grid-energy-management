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
        public static StatusNotifyMessage createEnergyRequestMessage(string dest, string source, PeerStatus peerStatus, float enReq)
        {
            StatusNotifyMessage message = new StatusNotifyMessage()
            {
                header = Tools.getHeader(dest, source, true),
                status = peerStatus,
                energyReq = enReq
            };

            return message;
        }

        public static EnergyProposalMessage createEnergyProposalMessage(string dest, string source, float enReq, float enPrice)
        {
            EnergyProposalMessage message = new EnergyProposalMessage()
            {
                header = Tools.getHeader(dest, source),
                /* TODO: Optimization required
                //If peer's energy is >= the request, give the requested energy, otherwise give the en. available
                energyAvailable = energyAvailable >= message.energyReq ? message.energyReq : energyAvailable,
                */
                energyAvailable = enReq,
                price = enPrice
            };

            return message;
        }

        public static EnergyAcceptMessage createEnergyAcceptMessage(string dest, string source, float qEnergy)
        {
            EnergyAcceptMessage message = new EnergyAcceptMessage()
            {
                header = Tools.getHeader(dest, source),
                energy = qEnergy
            };

            return message;
        }

        public static EndProposalMessage createEndProposalMessage(string dest, string source, bool status, float qEnergy)
        {
            EndProposalMessage message = new EndProposalMessage()
            {
                header = Tools.getHeader(dest, source),
                endStatus = status,
                energy = qEnergy
            };

            return message;
        }

        public static HeartBeatMessage createHeartBeatMessage(string source)
        {
            HeartBeatMessage message = new HeartBeatMessage()
            {
                header = Tools.getHeader("@all", source)
            };

            return message;
        }
    }
}
