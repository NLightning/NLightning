/*
 * Uses test vectors from
 * https://github.com/lightningnetwork/lightning-rfc/blob/master/03-transactions.md#appendix-d-per-commitment-secret-generation-test-vectors
 */

using System;
using NLightning.Utils.Extensions;
using NLightning.Wallet.KeyDerivation;
using Xunit;

namespace NLightning.Test.Wallet.KeyDerivation
{
    public class ShaChainTests
    {
        [Fact(DisplayName = "generate_from_seed 0 final node")]
        public void DeriveTest1()
        {
            byte[] data = "0000000000000000000000000000000000000000000000000000000000000000".HexToByteArray();
            ShaChain shaChain = new ShaChain(data);            
            var result = shaChain.Derive(0xffffffffffff);
            
            Assert.Equal("02a40c85b6f28da08dfdbe0926c53fab2de6d28c10301f8f7c4073d5e42e3148".HexToByteArray(), result);
        }
        
        [Fact(DisplayName = "generate_from_seed FF final node")]
        public void DeriveTest2()
        {
            byte[] data = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF".HexToByteArray();
            ShaChain shaChain = new ShaChain(data);            
            var result = shaChain.Derive(0xffffffffffff);
            
            Assert.Equal("7cc854b54e3e0dcdb010d7a3fee464a9687be6e8db3be6854c475621e007a5dc".HexToByteArray(), result);
        }
        
        [Fact(DisplayName = "generate_from_seed FF alternate bits 1")]
        public void DeriveTest3()
        {
            byte[] data = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF".HexToByteArray();
            ShaChain shaChain = new ShaChain(data);            
            var result = shaChain.Derive(0xaaaaaaaaaaa);
            
            Assert.Equal("56f4008fb007ca9acf0e15b054d5c9fd12ee06cea347914ddbaed70d1c13a528".HexToByteArray(), result);
        }
        
        [Fact(DisplayName = "generate_from_seed FF alternate bits 2")]
        public void DeriveTest4()
        {
            byte[] data = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF".HexToByteArray();
            ShaChain shaChain = new ShaChain(data);            
            var result = shaChain.Derive(0x555555555555);
            
            Assert.Equal("9015daaeb06dba4ccc05b91b2f73bd54405f2be9f217fbacd3c5ac2e62327d31".HexToByteArray(), result);
        }
        
        [Fact(DisplayName = "generate_from_seed 01 last nontrivial node")]
        public void DeriveTest5()
        {
            byte[] data = "0101010101010101010101010101010101010101010101010101010101010101".HexToByteArray();
            ShaChain shaChain = new ShaChain(data);            
            var result = shaChain.Derive(1);
            
            Assert.Equal("915c75942a26bb3a433a8ce2cb0427c29ec6c1775cfc78328b57f6ba7bfeaa9c".HexToByteArray(), result);
        }
       
        [Fact]
        public void DeriveBitTransformationsSameIndexTest()
        {
            ShaChain shaChain = new ShaChain(new byte[1]);            
            var result = shaChain.DeriveBitTransformations(12, 12);
            Assert.Empty(result);
        }
        
        [Fact]
        public void DeriveBitTransformationsTest()
        {
            ShaChain shaChain = new ShaChain(new byte[1]);
            var result = shaChain.DeriveBitTransformations(0, 2);
            Assert.Single(result);
            Assert.Equal(1, result[0]);
        }
        
        [Fact]
        public void DeriveBitTransformationsCantDeriveTest()
        {
            ShaChain shaChain = new ShaChain(new byte[1]);           
            Assert.Throws<InvalidOperationException>(() => shaChain.DeriveBitTransformations(12, 4));
        }
    }
}