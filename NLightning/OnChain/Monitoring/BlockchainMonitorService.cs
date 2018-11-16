using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NLightning.Network;
using NLightning.OnChain.Client;
using NLightning.OnChain.Configuration;
using NLightning.OnChain.Monitoring.Models;
using NLightning.Persistence;
using NLightning.Utils.Extensions;

namespace NLightning.OnChain.Monitoring
{
    public class BlockchainMonitorService : IBlockchainMonitorService, IDisposable
    {
        private readonly IBlockchainClientService _clientService;
        private readonly EventLoopScheduler _taskScheduler = new EventLoopScheduler();
        private readonly BlockchainConfiguration _configuration;
        private readonly Subject<Transaction> _byIdTransactionProvider = new Subject<Transaction>();
        private readonly Subject<Transaction> _spendingTransactionProvider = new Subject<Transaction>();
        private readonly Dictionary<string, ushort> _watchedTransactionIds = new Dictionary<string, ushort>();
        private readonly Dictionary<string, ushort> _watchedSpendingTransaction = new Dictionary<string, ushort>();
        private readonly ILogger _logger;
        private readonly LocalPersistenceContext _dbContext;
        private readonly object _syncLock = new object();
        private uint256 _lastBestBlockHash;
        
        public BlockchainMonitorService(IBlockchainClientService clientService, IConfiguration configuration, 
                                        ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory)
        {
            _dbContext = scopeFactory.CreateScopedService<LocalPersistenceContext>();
            _configuration = configuration.GetConfiguration<BlockchainConfiguration>();
            _logger = loggerFactory.CreateLogger(GetType());
            _clientService = clientService;
        }

        public IObservable<Transaction> ByTransactionIdProvider => _byIdTransactionProvider;
        public IObservable<Transaction> SpendingTransactionProvider => _spendingTransactionProvider;
        
        public void Initialize(NetworkParameters networkParameters)
        {
            InitTimer();
        }

        public void WatchForTransactionId(string transactionId, ushort confirmationMinimum)
        {
            _watchedTransactionIds.TryAdd(transactionId, confirmationMinimum);
        }

        public void WatchForSpendingTransaction(string transactionId, ushort outputIndex)
        {
            FindOrCreateLookup(transactionId, outputIndex);
            _watchedSpendingTransaction.TryAdd(transactionId, outputIndex);
        }

        private SpendingTransactionLookup FindOrCreateLookup(string transactionId, ushort outputIndex)
        {
            lock (_syncLock)
            {
                var lookup = _dbContext.SpendingTransactionLookups.SingleOrDefault(existingLookup =>
                    existingLookup.TransactionId == transactionId && existingLookup.OutputIndex == outputIndex);
                
                if (lookup == null)
                {
                    lookup = new SpendingTransactionLookup
                    {
                        TransactionId = transactionId,
                        OutputIndex = outputIndex,
                        LastBlockHeight = (uint)_clientService.GetBlockCount()
                    };
                    
                    _dbContext.SpendingTransactionLookups.Add(lookup);
                    _dbContext.SaveChanges();
                }
            
                return lookup;
            }
        }

        private void InitTimer()
        {
            _taskScheduler.SchedulePeriodic(_configuration.BlockchainMonitorInterval, CheckForNewTransactions);
        }

        private void CheckForNewTransactions()
        {
            if (_watchedTransactionIds.Count == 0)
            {
                return;
            }

            var currentBestBlockHash = _clientService.GetBestBlockHash();
            if (currentBestBlockHash == _lastBestBlockHash)
            {
                return;
            }

            CheckForNewTransactionsById();
            CheckForNewSpendingTransactions();
            _lastBestBlockHash = currentBestBlockHash;
        }

        private void CheckForNewSpendingTransactions()
        {
            if (_watchedSpendingTransaction.Count == 0)
            {
                return;
            }

            var startTime = DateTime.Now;
            var currentBlockHeight = _clientService.GetBlockCount();
            var lookups = _watchedSpendingTransaction.Select(txIdOutputIdx => FindOrCreateLookup(txIdOutputIdx.Key, txIdOutputIdx.Value)).ToList();
            var blockHeightStart = lookups.Select(l => (int) l.LastBlockHeight).Min();

            _logger.LogDebug($"Scan blocks ({blockHeightStart} to {currentBlockHeight}) for spending transactions ...");
            
            for (var i = blockHeightStart; i <= currentBlockHeight; i++)
            {
                var block = _clientService.GetBlock(i);
                
                foreach (var lookup in lookups.Where(l => l.LastBlockHeight < i))
                {
                    uint256 txId = uint256.Parse(lookup.TransactionId);
                    var spendingTx = block.Transactions.SingleOrDefault(tx =>
                        tx.Inputs.Exists(txIn => txIn.PrevOut.Hash == txId && txIn.PrevOut.N == lookup.OutputIndex));

                    if (spendingTx != null)
                    {
                        FoundSpendingTransaction(spendingTx);
                    }
                }
            }
            
            UpdateBlockHeights(lookups, currentBlockHeight);

            var duration = DateTime.Now - startTime;
            _logger.LogDebug($"Scan of blocks ({blockHeightStart} to {currentBlockHeight}) " +
                             $"for spending transactions took {duration.Seconds} sec.");
        }

        private void UpdateBlockHeights(List<SpendingTransactionLookup> lookups, int currentBlockHeight)
        {
            lock (_syncLock)
            {
                lookups.ForEach(lu => lu.LastBlockHeight = (uint) currentBlockHeight);
                _dbContext.SaveChanges();
            }
        }

        private void FoundSpendingTransaction(Transaction spendingTx)
        {
            _spendingTransactionProvider.OnNext(spendingTx);
        }

        private void CheckForNewTransactionsById()
        {
            foreach (var watchedTransactionId in _watchedTransactionIds.ToList())
            {
                var txInfo = _clientService.GetTransactionInfo(uint256.Parse(watchedTransactionId.Key));
                if (txInfo != null)
                {
                    if (txInfo.Confirmations >= watchedTransactionId.Value)
                    {
                        _watchedTransactionIds.Remove(watchedTransactionId.Key);
                        _byIdTransactionProvider.OnNext(txInfo.Transaction);
                    }
                }
            }
        }

        public void Dispose()
        {
            _taskScheduler.Dispose();
            _dbContext.Dispose();
            _spendingTransactionProvider.Dispose();
            _byIdTransactionProvider.Dispose();
        }
    }
}