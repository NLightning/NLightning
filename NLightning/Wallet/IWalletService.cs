using NBitcoin;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.OnChain;

namespace NLightning.Wallet
{
    public interface IWalletService
    {
        ECKeyPair Key { get; }
        BitcoinPubKeyAddress PubKeyAddress { get; }
        byte[] ShutdownScriptPubKey { get; }

        void Initialize(ECKeyPair key, NetworkParameters networkParameters);
    }
}