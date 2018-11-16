using NBitcoin;
using NLightning.Network;
using NLightning.Peer.Channel.Models;

namespace NLightning.Peer.Channel.Penalty
{
    public interface IPenaltyService
    {
        void Initialize(NetworkParameters networkParameters);
        void HandlePenalty(LocalChannel channel, Transaction revokedCommitmentTransaction);
    }
}