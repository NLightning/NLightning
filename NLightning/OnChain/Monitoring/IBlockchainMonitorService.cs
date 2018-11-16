using System;
using NBitcoin;
using NLightning.Network;

namespace NLightning.OnChain.Monitoring
{
    public interface IBlockchainMonitorService
    {
        IObservable<Transaction> ByTransactionIdProvider { get; }
        IObservable<Transaction> SpendingTransactionProvider { get; }
        
        void Initialize(NetworkParameters networkParameters);
        void WatchForTransactionId(string transactionId, ushort confirmationMinimum);
        void WatchForSpendingTransaction(string transactionId, ushort outputIndex);
    }
}