using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using NBitcoin;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.OnChain.Client;
using NLightning.Utils.Extensions;
using NLightning.Wallet;
using NLightning.Wallet.Funding;
using Xunit;

namespace NLightning.Test.Wallet.Funding
{
    public class FundingServiceTests
    {
        [Fact]
        public void CreateFundingTransactionNotEnoughFundsTest()
        {
            var keyService = new Mock<IWalletService>();
            var blockchainClientService = new Mock<IBlockchainClientService>();
            var fundingKeyPrivateKey = new ECKeyPair("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65", true);
            var pubKeyAddress = fundingKeyPrivateKey.ToPubKey().GetAddress(NBitcoin.Network.TestNet);
            var inputTx = Transaction.Parse("01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff03510101ffffffff0100f2052a010000001976a9143ca33c2e4446f4a305f23c80df8ad1afdcf652f988ac00000000", NBitcoin.Network.TestNet);

            keyService.Setup(k => k.Key).Returns(fundingKeyPrivateKey);
            keyService.Setup(k => k.PubKeyAddress).Returns(pubKeyAddress);
            blockchainClientService.Setup(b => b.ListUtxo(1, Int32.MaxValue, pubKeyAddress))
                .Returns(new List<Utxo>() {new Utxo()
                {
                    AmountSatoshi = 100000,
                    OutPoint = new OutPoint(inputTx, 0),
                    ScriptPubKey = inputTx.Outputs[0].ScriptPubKey
                }});
            
            FundingService fundingService = new FundingService(new LoggerFactory(), keyService.Object, blockchainClientService.Object, null);
            fundingService.Initialize(NetworkParameters.BitcoinTestnet);
            
            ECKeyPair pubKey1 = new ECKeyPair("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb", false);
            ECKeyPair pubKey2 = new ECKeyPair("030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c1", false);
            
            Assert.Throws<FundingException>(() => fundingService.CreateFundingTransaction(10000000, 1000, pubKey1, pubKey2));
        }
        
        [Fact]
        public void CreateFundingTransactionTest()
        {
            var keyService = new Mock<IWalletService>();
            var blockchainClientService = new Mock<IBlockchainClientService>();
            var secret = new BitcoinSecret("cRCH7YNcarfvaiY1GWUKQrRGmoezvfAiqHtdRvxe16shzbd7LDMz");

            var fundingKeyPrivateKey = new ECKeyPair(secret.PrivateKey.GetBytes(), true);
            var pubKeyAddress = fundingKeyPrivateKey.ToPubKey().GetAddress(NBitcoin.Network.TestNet);
            var inputTx = Transaction.Parse("01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff03510101ffffffff0100f2052a010000001976a9143ca33c2e4446f4a305f23c80df8ad1afdcf652f988ac00000000", NBitcoin.Network.TestNet);

            keyService.Setup(k => k.Key).Returns(fundingKeyPrivateKey);
            keyService.Setup(k => k.PubKeyAddress).Returns(pubKeyAddress);
            blockchainClientService.Setup(b => b.ListUtxo(1, Int32.MaxValue, pubKeyAddress))
                .Returns(new List<Utxo>() {new Utxo()
                {
                    AmountSatoshi = 100000000,
                    OutPoint = new OutPoint(inputTx, 0),
                    ScriptPubKey = inputTx.Outputs[0].ScriptPubKey
                }});
            
            FundingService fundingService = new FundingService(new LoggerFactory(), keyService.Object, blockchainClientService.Object, null);
            fundingService.Initialize(NetworkParameters.BitcoinTestnet);
            
            ECKeyPair pubKey1 = new ECKeyPair("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb", false);
            ECKeyPair pubKey2 = new ECKeyPair("030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c1", false);
            
            FundingTransaction fundingTransaction = fundingService.CreateFundingTransaction(10000000, 1000, pubKey1, pubKey2);

            Assert.Equal(1, fundingTransaction.FundingOutputIndex);
            Assert.Single(fundingTransaction.Transaction.Inputs);
            Assert.Equal(2, fundingTransaction.Transaction.Outputs.Count);
            Assert.Equal(89999765, fundingTransaction.Transaction.Outputs[0].Value.Satoshi);
            Assert.Equal(10000000, fundingTransaction.Transaction.Outputs[1].Value.Satoshi);
            Assert.Equal("35effff7a94b0fbd819b2feec4737d88a9c2f1a7cca481bd6fb206d23029c974", fundingTransaction.Transaction.GetHash().ToString());

        }
    }
}