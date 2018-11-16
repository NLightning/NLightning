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
    public class PrivateDerivationTests
    {
        private static readonly ECKeyPair BaseSecret          = new ECKeyPair("000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f", true);
        private static readonly ECKeyPair PerCommitmentPoint  = new ECKeyPair("025f7117a78150fe2ef97db7cfc83bd57b2e2c0d0dd25eaf467a4a1c2a45ce1486", false);
        
        [Fact(DisplayName = "derivation of private key from basepoint secret and per_commitment_secret")]
        public void DeriveTest()
        {
            var publicKeyDerivation = new PrivateKeyDerivation(PerCommitmentPoint);
           
            var resultKey = publicKeyDerivation.Derive(BaseSecret);
            Assert.Equal("cbced912d3b21bf196a766651e436aff192362621ce317704ea2f75d87e7be0f", resultKey.PrivateKeyData.ToHex());
        }
    }
}