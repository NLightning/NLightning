using NBitcoin;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.Utils.Extensions;

namespace NLightning.Wallet.KeyDerivation
{
    public class KeyDerivationService : IKeyDerivationService
    {
        private readonly IWalletService _walletService;
        private NetworkParameters _networkParameters;

        public KeyDerivationService(IWalletService walletService)
        {
            _walletService = walletService;
        }
        
        public void Initialize(NetworkParameters networkParameters)
        {
            _networkParameters = networkParameters;
        }
        
        public ECKeyPair DeriveKey(KeyFamily keyFamily, uint index)
        {
            ExtKey key = new ExtKey(_walletService.Key.PrivateKeyData);
            KeyPath keyPath = KeyPathHelper.Create(_networkParameters.CoinType, keyFamily, (int)index);
            ExtKey derivedKey = key.Derive(keyPath);

            return new ECKeyPair(derivedKey.PrivateKey.GetBytes(), true);
        }

        public ECKeyPair DerivePerCommitmentPoint(ECKeyPair revocationKey, uint commitmentIndex)
        {
            byte[] revocationRootHash = revocationKey.PrivateKey.D.ToByteArrayUnsigned();
            ShaChain shaChain = new ShaChain(revocationRootHash);
            byte[] firstPreImage = shaChain.Derive(commitmentIndex);
            return new ECKeyPair(firstPreImage, true);
        }
    }
}