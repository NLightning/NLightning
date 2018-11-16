using NLightning.Cryptography;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Cryptography
{
    public class ECDHTests
    {
        [Fact]
        public void Test()
        {
            byte[] key1 = "2121212121212121212121212121212121212121212121212121212121212121".HexToByteArray();
            ECKeyPair privateKey = new ECKeyPair(key1, true);
            byte[] key2 = "0242a4ae0c5bef18048fbecf995094b74bfb0f7391418d71ed394784373f41e4f3".HexToByteArray();
            ECKeyPair publicKey = new ECKeyPair(key2, false);

            byte[] result = ECDH.ComputeHashedPoint(publicKey.PublicKeyParameters, privateKey.PrivateKey);
            
            Assert.Equal("bd8d1d89b9ff4086baf9065df2562ccdd67399a5e9b2c0d427a92d83c1edd8fb", result.ToHex());
        }
    }
}