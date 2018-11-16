using NLightning.Cryptography;
using NLightning.Utils.Extensions;
using NLightning.Wallet.KeyDerivation;
using Xunit;

namespace NLightning.Test.Wallet.KeyDerivation
{
    public class PublicKeyDerivationTests
    {
        /*
         * Test Vectors:
         * https://github.com/lightningnetwork/lightning-rfc/blob/master/03-transactions.md#appendix-e-key-derivation-test-vectors
         */
        [Fact(DisplayName = "derivation of pubkey from basepoint and per_commitment_point")]
        public void DeriveTest()
        {
            var basePoint           = new ECKeyPair("036d6caac248af96f6afa7f904f550253a0f3ef3f5aa2fe6838a95b216691468e2", false);
            var perCommitmentPoint  = new ECKeyPair("025f7117a78150fe2ef97db7cfc83bd57b2e2c0d0dd25eaf467a4a1c2a45ce1486", false);
            var publicKeyDerivation = new PublicKeyDerivation(perCommitmentPoint);
           
            var resultKey = publicKeyDerivation.Derive(basePoint);
            Assert.Equal("0235f2dbfaa89b57ec7b055afe29849ef7ddfeb1cefdb9ebdc43f5494984db29e5" , resultKey.PublicKeyCompressed.ToHex());
        }
        
        [Fact]
        public void DeriveTest2()
        {
            var basepoint = new ECKeyPair("0319c56699c3ac4e7f2c3f1475e6374e08dfabafbbac852cd1578c9c45a61ded99", false);
            var perCommitmentPoint  = new ECKeyPair("031929dce9f9d1953000afab33d03517880ab37b8a15d0613401cf7a805737d537", false);
            var publicKeyDerivation = new PublicKeyDerivation(perCommitmentPoint);
           
            var resultKey = publicKeyDerivation.Derive(basepoint);
            Assert.Equal("034DF2D703AD06243285F07B7CBFF7B309F996961244818EBF4B8BD58CF6862CAE".ToLower(), resultKey.PublicKeyCompressed.ToHex());
        }
        
        [Fact]
        public void DeriveTest3()
        {
            var basepoint = new ECKeyPair("03a5d8c82ddbb1c42379abb9f09b7cafb96148351212a101a4f3ff1d113735be5e", false);
            var perCommitmentPoint  = new ECKeyPair("031929dce9f9d1953000afab33d03517880ab37b8a15d0613401cf7a805737d537", false);
            var publicKeyDerivation = new PublicKeyDerivation(perCommitmentPoint);
           
            var resultKey = publicKeyDerivation.Derive(basepoint);
            Assert.Equal("0247f4bedc1d76bd3f46812a61c5ed235e2f76493b3ea1aa7872e7ca4ac582f8b1".ToLower(), resultKey.PublicKeyCompressed.ToHex());
        }
                
        [Fact]
        public void DeriveTest4()
        {
            var basepoint = new ECKeyPair("03f4b6c6ada5f409c1a3de16edec3c84d93576c5c206d4e646f96e03ba9fa636e0", false);
            var perCommitmentPoint  = new ECKeyPair("02b0c45d17a7d72b02ace2fd4fda33f8a9d0274560ccbec6f9c43d6af17e309b47", false);
            var publicKeyDerivation = new PublicKeyDerivation(perCommitmentPoint);
           
            var resultKey = publicKeyDerivation.Derive(basepoint);
            Assert.Equal("02201305100A05C0408392869DFF1105702437BD8FB07B3AA49DA84C2FC2383427".ToLower(), resultKey.PublicKeyCompressed.ToHex());
        }
                        
        [Fact]
        public void DeriveTest5()
        {
            var basepoint = new ECKeyPair("0303a12202e9020ebc9995391a286b2c364bfbe1a107d4fac772d148e932d994e2", false);
            var perCommitmentPoint  = new ECKeyPair("02b0c45d17a7d72b02ace2fd4fda33f8a9d0274560ccbec6f9c43d6af17e309b47", false);
            var publicKeyDerivation = new PublicKeyDerivation(perCommitmentPoint);
           
            var resultKey = publicKeyDerivation.Derive(basepoint);
            Assert.Equal("02884DF47298A3B545E0DD278755C09A8D1D18A9C30E71F09CE80EC75939415068".ToLower(), resultKey.PublicKeyCompressed.ToHex());
        }
    }
}