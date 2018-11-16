using NBitcoin;
using NLightning.Network;
using NLightning.OnChain.Client;
using NLightning.Peer.Channel.Models;

namespace NLightning.Peer.Channel.Penalty
{
    public class PenaltyService : IPenaltyService
    {
        private readonly IBlockchainClientService _blockchainClientService;

        public PenaltyService(IBlockchainClientService blockchainClientService)
        {
            _blockchainClientService = blockchainClientService;
        }
        
        public void Initialize(NetworkParameters networkParameters)
        {
            
        }

        public void HandlePenalty(LocalChannel channel, Transaction revokedCommitmentTransaction)
        {
            
        }
    }
}