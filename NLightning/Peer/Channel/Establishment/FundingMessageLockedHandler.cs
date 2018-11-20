using NLightning.Peer.Channel.Establishment.Messages;
using NLightning.Peer.Channel.Models;

namespace NLightning.Peer.Channel.Establishment
{
    public class FundingMessageLockedHandler
    {
        public void Handle(FundingLockedMessage message, LocalChannel channel)
        {
            channel.State = channel.State == LocalChannelState.FundingLocked ? LocalChannelState.NormalOperation : LocalChannelState.FundingLocked;
            channel.RemoteCommitmentTxParameters.NextPerCommitmentPoint = message.NextPerCommitmentPoint;
        }
    }
}