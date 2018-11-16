using Moq;
using NBitcoin;
using NLightning.Cryptography;
using NLightning.OnChain;
using NLightning.Utils.Extensions;
using NLightning.Wallet;
using NLightning.Wallet.Funding;
using Xunit;

namespace NLightning.Test.Wallet.Funding
{
    public class FundingServiceTests
    {
        [Fact]
        public void CreateFundingTransactionTest()
        {
//            var keyService = new Mock<IKeyService>();
//            var secret = new BitcoinSecret("cRCH7YNcarfvaiY1GWUKQrRGmoezvfAiqHtdRvxe16shzbd7LDMz");
//            
//            keyService.Setup(k => k.Key).Returns(secret.PrivateKey);
//            
//            TransactionSigningService signingService = new TransactionSigningService(keyService.Object);
//            FundingService fundingService = new FundingService(null, keyService.Object, signingService, null);
//
//            fundingService.Initialize(Blockchain.BitcoinTestnet);
//            
//            ECKeyPair pubKey1 = new ECKeyPair("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb", false);
//            ECKeyPair pubKey2 = new ECKeyPair("030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c1", false);
//            Transaction input = Transaction.Parse("01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff03510101ffffffff0100f2052a010000001976a9143ca33c2e4446f4a305f23c80df8ad1afdcf652f988ac00000000", NBitcoin.Network.TestNet);
//            
//            (Transaction fundingTransaction, ushort fundingOutputIndex) = fundingService.CreateFundingTransaction(10000000, 1000, pubKey1, pubKey2);
//
//            Assert.Equal(1, fundingOutputIndex);
//            Assert.Single(fundingTransaction.Inputs);
//            Assert.Equal(2, fundingTransaction.Outputs.Count);
//            Assert.Equal(10000000, fundingTransaction.Outputs[fundingOutputIndex].Value.Satoshi);
        }
    }
}