using NLightning.Cryptography;
using NLightning.Network;

namespace NLightning.Wallet.KeyDerivation
{
    public interface IKeyDerivationService
    {
        void Initialize(NetworkParameters networkParameters);
        ECKeyPair DeriveKey(KeyFamily keyFamily, uint index);
        ECKeyPair DerivePerCommitmentPoint(ECKeyPair revocationKey, uint commitmentIndex);
    }
}