using NLightning.Cryptography;
using NLightning.Utils.Extensions;
using NLightning.Wallet;
using Xunit;

namespace NLightning.Test.Wallet
{
    public class MultiSignaturePubKeyTests
    {
        [Fact]
        public void GenerateScriptPubKeyTest()
        {
            ECKeyPair pubKey1 = new ECKeyPair("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb", false);
            ECKeyPair pubKey2 = new ECKeyPair("030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c1", false);
            
            var result =  MultiSignaturePubKey.GenerateMultisigPubKey(pubKey1, pubKey2);
            Assert.Equal("5221023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb21030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c152ae", 
                result.ToBytes().ToHex());
        }
        
        [Fact]
        public void GenerateScriptPubKeyLexicographicOrderingTest()
        {
            ECKeyPair pubKey1 = new ECKeyPair("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb", false);
            ECKeyPair pubKey2 = new ECKeyPair("030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c1", false);
            
            var result =  MultiSignaturePubKey.GenerateMultisigPubKey(pubKey2, pubKey1);
            Assert.Equal("5221023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb21030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c152ae", 
                result.ToBytes().ToHex());
            
            var result2 =  MultiSignaturePubKey.GenerateMultisigPubKey(pubKey1, pubKey2);
            Assert.Equal("5221023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb21030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c152ae", 
                result2.ToBytes().ToHex());
        }
    }
}