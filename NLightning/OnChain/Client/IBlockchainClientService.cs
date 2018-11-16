using System.Collections.Generic;
using NBitcoin;
using NLightning.Cryptography;
using NLightning.Network;

namespace NLightning.OnChain.Client
{
    public interface IBlockchainClientService
    {
        void Initialize(ECKeyPair walletKey, NetworkParameters networkParameters);
        uint GetFeeRatePerKw(int confirmationTarget);
        void SendTransaction(Transaction transaction);
        uint256 GetBestBlockHash();
        int GetBlockCount();
        TransactionInfo GetTransactionInfo(uint256 transactionId);
        List<Utxo> ListUtxo(int confirmationMinimum, int confirmationMaximum, params BitcoinAddress[] addresses);
        Block GetBlock(int height);
    }
}