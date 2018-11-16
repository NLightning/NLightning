using NBitcoin;
using NLightning.Network;
using NLightning.Peer.Channel.Models;

namespace NLightning.Peer.Channel
{
    public interface IUnilateralCloseService
    {
        void Initialize(NetworkParameters networkParameters);
        void HandleUnilateralClose(LocalChannel channel, Transaction confirmedCommitmentTransaction);
        void HandleNewerCommitmentTransaction(LocalChannel channel, Transaction confirmedCommitmentTransaction);
    }
}