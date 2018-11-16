using System;
using NLightning.Cryptography;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Cryptography
{
    public class Chacha20Poly1305Tests
    {
        [Fact]
        public void EncryptWithADTestWithoutPlainTextMacTest()
        {
            String key = "e68f69b7f096d7917245f5e5cf8ae1595febe4d4644333c99f9c4a1282031c9f";
            String additionalData = "9e0e7de8bb75554f21db034633de04be41a2b8a18da7a319a03c803bf02b396c";
            String expectedCipherResult = "0df6086551151f58b8afe6c195782c6a";

            Nonce nonce = new Nonce();

            (byte[] actualCipherText, byte[] mac) = ChaCha20Poly1305.EncryptWithAdditionalData(key.HexToByteArray(), nonce.GetBytes(), additionalData.HexToByteArray(), new byte[0]); 
            Assert.Equal(new byte[0], actualCipherText);
            Assert.Equal(expectedCipherResult, mac.ToHex());
        }
        
        [Fact]
        public void EncryptWithADTestWithPlainText()
        {
            String key = "908b166535c01a935cf1e130a5fe895ab4e6f3ef8855d87e9b7581c4ab663ddc";
            String additionalData = "90578e247e98674e661013da3c5c1ca6a8c8f48c90b485c0dfa1494e23d56d72";
            String plaintext = "034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa";
            String expectedCipherResult = "b9e3a702e93e3a9948c2ed6e5fd7590a6e1c3a0344cfc9d5b57357049aa22355361aa02e55a8fc28fef5bd6d71ad0c3822";

            Nonce nonce = new Nonce();
            nonce.Increment();

            (byte[] actualCipherText, byte[] mac) = ChaCha20Poly1305.EncryptWithAdditionalData(key.HexToByteArray(), nonce.GetBytes(), additionalData.HexToByteArray(), plaintext.HexToByteArray()); 
            
            Assert.Equal(expectedCipherResult, actualCipherText.ToHex() + mac.ToHex());
        }
    }
}