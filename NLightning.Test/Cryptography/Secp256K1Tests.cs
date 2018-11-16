using NLightning.Cryptography;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Cryptography
{
    public class Secp256K1Tests
    {
        [Fact]
        public void GenerateKeyPairTest()
        {
            var ecKeyPair = Secp256K1.GenerateKeyPair();
            
            Assert.True(ecKeyPair.HasPrivateKey);
            Assert.Equal(33, ecKeyPair.PublicKeyCompressed.Length);
        }
        
        [Fact]
        public void VerifySignatureTest()
        {
            byte[] witness = "6b32bec9a3aeda57863bdc41d880f47944bf234a2628b084b46d4351d88195d2".HexToByteArray();
            byte[] signature = "fe531b7551494341543449bce877275b7c4f2e7ad416279fc7aa732af90ccb9339fa3c2d59dd2c8b4cc68957d59bfc5186d164739a8b37d450ab1fcb50700322".HexToByteArray();
            ECKeyPair publicKey = new ECKeyPair("03933884aaf1d6b108397e5efe5c86bcf2d8ca8d2f700eda99db9214fc2712b134", false);
            
            Assert.True(Secp256K1.VerifySignature(witness, signature, publicKey));
        }
        
        [Fact]
        public void VerifyInvalidSignatureTest()
        {
            byte[] witness = "6b32bec9a3aeda57863bdc41d880f47944bf234a2628b084b46d4351d88195d2".HexToByteArray();
            byte[] signature = "fe531b7551494341543459bce877275b7c4f2e7ad416279fc7aa732af90ccb9339fa3c2d59dd2c8b4cc68957d59bfc5186d164739a8b37d450ab1fcb50700322".HexToByteArray();
            ECKeyPair publicKey = new ECKeyPair("03933884aaf1d6b108397e5efe5c86bcf2d8ca8d2f700eda99db9214fc2712b134", false);
            
            Assert.False(Secp256K1.VerifySignature(witness, signature, publicKey));
        }
    }
}