using System;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.OnChain;

namespace NLightning.Wallet
{
    public class WalletService : IWalletService
    {
        private ECKeyPair _key;
        private NetworkParameters _networkParameters;
        private ILogger _logger;

        public WalletService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }
        
        public ECKeyPair Key => _key;
        public BitcoinPubKeyAddress PubKeyAddress => _key.ToPubKey().GetAddress(NBitcoin.Network.GetNetwork(_networkParameters.Name));
        public byte[] ShutdownScriptPubKey => PubKeyAddress.ScriptPubKey.ToBytes();

        public void Initialize(ECKeyPair key, NetworkParameters networkParameters)
        {
            _key = key;
            _networkParameters = networkParameters;
            Console.WriteLine($"Bitcoin Address: {PubKeyAddress}"); 
        }
    }
}