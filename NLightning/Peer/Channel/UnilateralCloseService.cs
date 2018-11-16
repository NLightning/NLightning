using NBitcoin;
using NLightning.Network;
using NLightning.Peer.Channel.Models;

namespace NLightning.Peer.Channel
{
    public class UnilateralCloseService : IUnilateralCloseService
    {
        public void Initialize(NetworkParameters networkParameters)
        {
            
        }

        public void HandleUnilateralClose(LocalChannel channel, Transaction confirmedCommitmentTransaction)
        {
            
        }

        public void HandleNewerCommitmentTransaction(LocalChannel channel, Transaction confirmedCommitmentTransaction)
        {
            
        }
    }
}