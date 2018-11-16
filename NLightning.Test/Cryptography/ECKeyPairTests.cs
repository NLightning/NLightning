using NLightning.Cryptography;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Cryptography
{
    public class ECKeyPairTests
    {
        [Fact]
        public void ToPrivateKeyTest()
        {
            byte[] key = "2121212121212121212121212121212121212121212121212121212121212121".HexToByteArray();
            ECKeyPair ecKeyPair = new ECKeyPair(key, true);

            var privateKey = ecKeyPair.ToPrivateKey();

            Assert.Equal(key.ToHex(), privateKey.GetBytes().ToHex());
        }
        
        [Fact]
        public void ToPubKeyTest()
        {
            byte[] key = "0242a4ae0c5bef18048fbecf995094b74bfb0f7391418d71ed394784373f41e4f3".HexToByteArray();
            ECKeyPair ecKeyPair = new ECKeyPair(key, false);

            var pubKey = ecKeyPair.ToPubKey();

            Assert.Equal(key.ToHex(), pubKey.ToHex());
        }

                
        [Fact]
        public void PublicKeyCompressedTest()
        {
            byte[] key = "0242a4ae0c5bef18048fbecf995094b74bfb0f7391418d71ed394784373f41e4f3".HexToByteArray();
            ECKeyPair ecKeyPair = new ECKeyPair(key, false);

            var compressed = ecKeyPair.PublicKeyCompressed.ToHex();

            Assert.Equal(key.ToHex(), compressed);
        }

        [Fact]
        public void PrivateKeyDataTest()
        {
            byte[] key = "2121212121212121212121212121212121212121212121212121212121212121".HexToByteArray();
            ECKeyPair ecKeyPair = new ECKeyPair(key, true);

            Assert.Equal(key.ToHex(), ecKeyPair.PrivateKeyData.ToHex());
        }

        
        [Fact]
        public void LexicographicalOrderingTest()
        {
            ECKeyPair key1 = new ECKeyPair("0242a4ae0c5bef18048fbecf995094b74bfb0f7391418d71ed394784373f41e4f3", false);
            ECKeyPair key2 = new ECKeyPair("028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7", false);
            
            Assert.True(key1 < key2);
            Assert.True(key2 > key1);
            
            Assert.Equal(-4, key1.CompareTo(key2));
            Assert.Equal(4, key2.CompareTo(key1));
            Assert.Equal(0, key2.CompareTo(key2));
        }
    }
}