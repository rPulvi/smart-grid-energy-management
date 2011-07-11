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
        public static HelloMessage CreateHelloMessage(string dest, string source, PeerStatus status, EnergyType enType, 
            float enProduced, float enPeak, float enPrice, string address, string admin)
        {
            HelloMessage message = new HelloMessage()
            {
                header = Tools.getHeader(dest, source),
                Status = status,
                EnType = enType,
                EnProduced = enProduced,
                EnPeak = enPeak,
                EnPrice = enPrice,
                Address = address,
                Admin = admin
            };

            return message;
        }

        public static HelloResponseMessage createHelloResponseMessage(string dest, string source, string resName)
        {
            HelloResponseMessage message = new HelloResponseMessage()
            {
                header = Tools.getHeader(dest, source),
                ResolverName = resName
            };

            return message;
        }

        public static StatusNotifyMessage createEnergyRequestMessage(string dest, string source, PeerStatus peerStatus, float enReq)
        {
            StatusNotifyMessage message = new StatusNotifyMessage()
            {
                header = Tools.getHeader(dest, source),
                status = peerStatus,
                energyReq = enReq
            };

            return message;
        }

        public static EnergyProposalMessage createEnergyProposalMessage(Guid SessionID, string dest, string source, float enReq, float enPrice)
        {
            EnergyProposalMessage message = new EnergyProposalMessage()
            {
                header = Tools.getHeader(dest, source, SessionID),
                energyAvailable = enReq,
                price = enPrice
            };

            return message;
        }

        public static EnergyAcceptMessage createEnergyAcceptMessage(Guid SessionID, string dest, string source, string pName, float qEnergy)
        {
            EnergyAcceptMessage message = new EnergyAcceptMessage()
            {
                header = Tools.getHeader(dest, source, SessionID),
                peerName = pName,
                energy = qEnergy
            };

            return message;
        }

        public static EndProposalMessage createEndProposalMessage(Guid SessionID, string dest, string source, bool status, float qEnergy, float enPrice)
        {
            EndProposalMessage message = new EndProposalMessage()
            {
                header = Tools.getHeader(dest, source, SessionID),
                endStatus = status,
                energy = qEnergy,
                price = enPrice
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

        public static UpdateStatusMessage createUpdateStatusMessage(string dest, string source, float enSold, float enBought)
        {
            UpdateStatusMessage message = new UpdateStatusMessage()
            {
                header = Tools.getHeader(dest,source),                
                energySold = enSold,
                energyBought = enBought
            };

            return message;
        }

        public static PeerIsDownMessage createPeerIsDownMessage(string dest, string source, string peerDownName)
        {
            PeerIsDownMessage message = new PeerIsDownMessage()
            {
                header = Tools.getHeader(dest, source),
                peerName = peerDownName
            };

            return message;
        }

        public static RemoteEnergyRequest createRemoteEnergyRequestMessage(StatusNotifyMessage energyRequestMessage, string dest, string source, string resolverIP, string resolverPort)
        {
            RemoteEnergyRequest message = new RemoteEnergyRequest()
            {
                header = Tools.getHeader(dest, source),
                enReqMessage = energyRequestMessage,
                IP = resolverIP,
                port = resolverPort
            };

            return message;
        }

        public static RemoteEndProposalMessage createRemoteEndProposalMessage(EndProposalMessage endPropMessage, string dest, string source, string resolverIP, string resolverPort)
        {
            RemoteEndProposalMessage message = new RemoteEndProposalMessage()
            {
                header = Tools.getHeader(dest, source),
                endProposalMessage = endPropMessage,
                IP = resolverIP,
                port = resolverPort
            };

            return message;
        }
    }
}
