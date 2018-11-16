using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NBitcoin.RPC;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.OnChain;
using NLightning.OnChain.Client;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Models;
using NLightning.Wallet.Commitment;

namespace NLightning.Wallet.Funding
{
    public class FundingService : IFundingService
    {
        private readonly ILogger _logger;
        private readonly IWalletService _walletService;
        private readonly IBlockchainClientService _blockchainClientService;
        private readonly IChannelLoggingService _channelLoggingService;
        private NetworkParameters _networkParameters;

        public FundingService(ILoggerFactory loggerFactory,IWalletService walletService, IBlockchainClientService blockchainClientService,
                                IChannelLoggingService channelLoggingService)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _walletService = walletService;
            _blockchainClientService = blockchainClientService;
            _channelLoggingService = channelLoggingService;
        }
        
        public void Initialize(NetworkParameters networkParameters)
        {
            _networkParameters = networkParameters;
        }

        public FundingTransaction CreateFundingTransaction(ulong amount, ulong feeRate, ECKeyPair pubKey1, ECKeyPair pubKey2)
        {
            var script = MultiSignaturePubKey.GenerateMultisigPubKey(pubKey1, pubKey2);
            var redeemScript = script.WitHash.ScriptPubKey;
            var unspent = _blockchainClientService.ListUtxo(1, Int32.MaxValue, _walletService.PubKeyAddress);
            
            _logger.LogInformation("Transaction Funding: found " + unspent.Count + " UTXO to fund the funding transaction");
            var totalAmount = (ulong)unspent.Select(u => u.AmountSatoshi).Sum();
            var fee = TransactionFee.CalculateFee(feeRate, 1000);
            var neededAmount = fee + amount;
                
            if (neededAmount > totalAmount)
            {
                throw new FundingException($"Not enough funds. Needed at least {neededAmount} Satoshi, but only {totalAmount} Satoshi are available.");
            }

            (Transaction fundingTransaction, List<Coin> inputCoins) = CreateFundingTransactionFromUtxo(amount, unspent, script, feeRate);
            ushort fundingOutputIndex = (ushort)fundingTransaction.Outputs.FindIndex(o => o.ScriptPubKey == redeemScript);
            
            return new FundingTransaction(fundingTransaction, fundingOutputIndex, inputCoins, _walletService.PubKeyAddress.ScriptPubKey.ToBytes());
        }

        private (Transaction unsigned, List<Coin> inputCoins) CreateFundingTransactionFromUtxo(ulong amount, List<Utxo> unspent, Script script, ulong feeRate)
        {
            TransactionBuilder builder = new TransactionBuilder();
            var redeemScript = script.WitHash.ScriptPubKey;
            var coins = unspent.Select(utxo => new Coin(utxo.OutPoint, new TxOut(utxo.AmountSatoshi, utxo.ScriptPubKey))).ToList();

            var tx = builder
                .AddCoins(coins)
                .Send(redeemScript, Money.Satoshis(amount))
                .SetChange(_walletService.PubKeyAddress)
                .AddKeys(_walletService.Key.ToPrivateKey())
                .SendEstimatedFees(new FeeRate(Money.Satoshis(feeRate)))
                .SetConsensusFactory(_networkParameters.Network)
                .BuildTransaction(true);

            if (!builder.Verify(tx))
            {
                throw new FundingException("Funding transaction is invalid");
            }
            
            return (tx, coins);
        }

        public void BroadcastFundingTransaction(LocalChannel channel)
        {
            var fundingTransaction = CreateFundingTransaction(channel.FundingSatoshis, channel.FeeRatePerKw,
                channel.LocalCommitmentTxParameters.FundingKey, channel.RemoteCommitmentTxParameters.FundingKey);
            
            _channelLoggingService.LogInfo(channel, "Funding Transaction", fundingTransaction.ToString());
            _blockchainClientService.SendTransaction(fundingTransaction.Transaction);
        }
    }
}