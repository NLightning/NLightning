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
    public class RevocationPublicKeyTests
    {
        [Fact(DisplayName = "derivation of revocation pubkey from basepoint and per_commitment_point")]
        public void DerivePublicKeyTest()
        {
            var basePoint           = new ECKeyPair("036d6caac248af96f6afa7f904f550253a0f3ef3f5aa2fe6838a95b216691468e2", false);
            var perCommitmentPoint  = new ECKeyPair("025f7117a78150fe2ef97db7cfc83bd57b2e2c0d0dd25eaf467a4a1c2a45ce1486", false);
            var publicKeyDerivation = new RevocationPublicKeyDerivation(perCommitmentPoint);
           
            var resultKey = publicKeyDerivation.DerivePublicKey(basePoint);
            Assert.Equal("02916e326636d19c33f13e8c0c3a03dd157f332f3e99c317c141dd865eb01f8ff0", resultKey.PublicKeyCompressed.ToHex());
        }
        
        [Fact]
        public void DerivePublicKeyTest2()
        {
            var basePoint           = new ECKeyPair("03556bc751fe83e1a6e37fe009d6af41c1f757aa53e8fc4266a6dbf4ab7aaad974", false);
            var perCommitmentPoint  = new ECKeyPair("031929dce9f9d1953000afab33d03517880ab37b8a15d0613401cf7a805737d537", false);
            var publicKeyDerivation = new RevocationPublicKeyDerivation(perCommitmentPoint);
           
            var resultKey = publicKeyDerivation.DerivePublicKey(basePoint);
            Assert.Equal("021B279460B0CBDA8BF1F15A99B07101585EE6259555818C1D19CA8BAC4A177672".ToLower(), resultKey.PublicKeyCompressed.ToHex());
        }
    }
}