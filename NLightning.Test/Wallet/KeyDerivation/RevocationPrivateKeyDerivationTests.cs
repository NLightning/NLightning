using NLightning.Cryptography;
using NLightning.Utils.Extensions;
using NLightning.Wallet.KeyDerivation;
using Xunit;

namespace NLightning.Test.Wallet.KeyDerivation
{
    /*
     * Test Vectors:
     * https://github.com/lightningnetwork/lightning-rfc/blob/master/03-transactions.md#appendix-e-key-derivation-test-vectors
     */
    public class RevocationPrivateKeyTests
    {
        private static readonly ECKeyPair BaseSecret          = new ECKeyPair("000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f", true);
        private static readonly ECKeyPair PerCommitmentSecret = new ECKeyPair("1f1e1d1c1b1a191817161514131211100f0e0d0c0b0a09080706050403020100", true);
        
        [Fact(DisplayName = "derivation of revocation secret from basepoint_secret and per_commitment_secret")]
        public void DerivePrivateKeyTest()
        {
            var publicKeyDerivation = new RevocationPrivateKeyDerivation(PerCommitmentSecret);
            var resultKey = publicKeyDerivation.DerivePrivateKey(BaseSecret);
            
            Assert.Equal("d09ffff62ddb2297ab000cc85bcb4283fdeb6aa052affbc9dddcf33b61078110", resultKey.PrivateKeyData.ToHex());
        }
    }
}