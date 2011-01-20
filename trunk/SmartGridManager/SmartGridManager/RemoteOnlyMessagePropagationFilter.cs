using System.ServiceModel;
using System.ServiceModel.Channels;

namespace SmartGridManager
{
    public class RemoteOnlyMessagePropagationFilter : PeerMessagePropagationFilter
    {
        public RemoteOnlyMessagePropagationFilter() { }

        public override PeerMessagePropagation ShouldPropagateMessage(Message message, PeerMessageOrigination origination)
        {
            PeerMessagePropagation destination = PeerMessagePropagation.LocalAndRemote;

            if (origination == PeerMessageOrigination.Local)
                destination = PeerMessagePropagation.Remote;

            return destination;
        }

    }
}
