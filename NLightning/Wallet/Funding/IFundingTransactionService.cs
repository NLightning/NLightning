using NLightning.Cryptography;
using NLightning.Network;
using NLightning.OnChain;
using NLightning.Peer.Channel.Models;

namespace NLightning.Wallet.Funding
{
    public interface IFundingService
    {
        void Initialize(NetworkParameters networkParameters);
        FundingTransaction CreateFundingTransaction(ulong amount, ulong feeRate, ECKeyPair pubKey1, ECKeyPair pubKey2);
        void BroadcastFundingTransaction(LocalChannel channel);
    }
}