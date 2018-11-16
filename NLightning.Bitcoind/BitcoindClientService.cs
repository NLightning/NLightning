using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NBitcoin.RPC;
using NLightning.Bitcoind.Configuration;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.OnChain.Client;
using NLightning.Utils.Extensions;

namespace NLightning.Bitcoind
{
    public class BitcoindClientService : IBlockchainClientService
    {
        private const string SecretLabel = "NLightning Wallet";
        private readonly BitcoindClientConfiguration _configuration;
        private readonly ILogger _logger;
        private RPCClient _client;
        
        public BitcoindClientService(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _configuration = configuration.GetConfiguration<BitcoindClientConfiguration>();
        }
        
        public void Initialize(ECKeyPair key, NetworkParameters networkParameters)
        {
            CreateClient(networkParameters);

            var secret = key.ToPrivateKey().GetBitcoinSecret(NBitcoin.Network.GetNetwork(networkParameters.Name));

            try
            {
                bool alreadyImported = HasImportedKey(secret);

                if (!alreadyImported)
                {
                    ImportPrivateKey(secret, SecretLabel, true);
                }
            }
            catch (WebException webException)
            {
                string error = "Initialize failed: bitcoind is not available. ";
                _logger.LogError(error + webException, webException);
                throw new BlockchainClientException(error, webException);
            }
        }
        
        private void ImportPrivateKey(BitcoinSecret bitcoinSecret, string label, bool rescan)
        {
            _logger.LogInformation("bitcoind: Importing private key. This will take several minutes.");
            _client.ImportPrivKeyAsync(bitcoinSecret, label, rescan)
                .ContinueWith(t => _logger.LogInformation("bitcoind: Import of private key completed."));
        }

        private bool HasImportedKey(BitcoinSecret bitcoinSecret)
        {
            return _client.ListSecrets().Any(secret => bitcoinSecret.ToString() == secret.ToString());
        }

        public uint GetFeeRatePerKw(int confirmationTarget)
        {
            return (uint)_client.EstimateSmartFee(confirmationTarget).FeeRate.FeePerK.Satoshi;
        }

        public void SendTransaction(Transaction transaction)
        {
            _logger.LogInformation($"bitcoind: Send Transaction: {transaction}");
            _client.SendRawTransaction(transaction);
        }
        
        public List<Utxo> ListUtxo(int confirmationMinimum, int confirmationMaximum, params BitcoinAddress[] addresses)
        {
            return _client.ListUnspent(confirmationMinimum, confirmationMaximum, addresses)
                        .Select(unspent => new Utxo
                                            {
                                                AmountSatoshi = unspent.Amount.Satoshi, 
                                                OutPoint = unspent.OutPoint, 
                                                ScriptPubKey = unspent.ScriptPubKey
                                            }).ToList();
        }

        public Block GetBlock(int blockHeight)
        {
            return _client.GetBlock(blockHeight);
        }

        public uint256 GetBestBlockHash()
        {
            return _client.GetBestBlockHash();
        }
        
        public int GetBlockCount()
        {
            return _client.GetBlockCount();
        }

        public TransactionInfo GetTransactionInfo(uint256 transactionId)
        {
            try
            {
                var rawInfo = _client.GetRawTransactionInfo(transactionId);
                return new TransactionInfo
                {
                    Transaction = rawInfo.Transaction,
                    Confirmations = rawInfo.Confirmations
                };
            }
            catch (RPCException exception)
            {
                _logger.LogDebug($"Transaction {transactionId} not found: {exception}");
                return null;
            }
        }

        public void ScanTxOutSet(string inputHash)
        {
            //_client.StartScanTxoutSet(ScanTx)
        }

        private void CreateClient(NetworkParameters networkParameters)
        {
            var credentialString = new RPCCredentialString();
            var uri = $"http://{_configuration.RpcIpAddress}:{_configuration.RpcPort}";
            var network = NBitcoin.Network.GetNetwork(networkParameters.Name);

            credentialString.UserPassword = new NetworkCredential(_configuration.RpcUser, _configuration.RpcPassword);
            
            _client = new RPCClient(credentialString, uri, network);
        }
    }
}