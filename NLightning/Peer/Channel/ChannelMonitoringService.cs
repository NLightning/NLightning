using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using NBitcoin;
using NLightning.Network;
using NLightning.OnChain.Monitoring;
using NLightning.Peer.Channel.Establishment;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Models;
using NLightning.Peer.Channel.Penalty;
using NLightning.Wallet.Commitment;

namespace NLightning.Peer.Channel
{
    public class ChannelMonitoringService : IChannelMonitoringService, IDisposable
    {
        private readonly IChannelService _channelService;
        private readonly IChannelEstablishmentService _channelEstablishmentService;
        private readonly IBlockchainMonitorService _blockchainMonitorService;
        private readonly IChannelLoggingService _channelLoggingService;
        private readonly IPenaltyService _penaltyService;
        private readonly IUnilateralCloseService _unilateralCloseService;
        private readonly EventLoopScheduler _taskScheduler = new EventLoopScheduler();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        
        public ChannelMonitoringService(IChannelService channelService,
            IChannelEstablishmentService channelEstablishmentService,
            IBlockchainMonitorService blockchainMonitorService,
            IChannelLoggingService channelLoggingService,
            IPenaltyService penaltyService,
            IUnilateralCloseService unilateralCloseService)
        {
            _channelService = channelService;
            _channelEstablishmentService = channelEstablishmentService;
            _blockchainMonitorService = blockchainMonitorService;
            _channelLoggingService = channelLoggingService;
            _penaltyService = penaltyService;
            _unilateralCloseService = unilateralCloseService;
        }

        public void Initialize(NetworkParameters networkParameters)
        {
            SubscribeToEvents();
            MonitorChannels();
        }

        private void SubscribeToEvents()
        {
            _subscriptions.Add(_channelEstablishmentService.SuccessProvider
                .ObserveOn(_taskScheduler)
                .Subscribe(peerChannel => MonitorChannel(peerChannel.Channel)));
            
            _subscriptions.Add(_blockchainMonitorService.SpendingTransactionProvider
                .ObserveOn(_taskScheduler)
                .Subscribe(OnSpendingTransaction));
        }

        private void OnSpendingTransaction(Transaction spendingTx)
        {
            string fundingTxId = spendingTx.Inputs[0].PrevOut.Hash.ToString();
            uint fundingTxOutputIndex = spendingTx.Inputs[0].PrevOut.N;
            var channel = _channelService.Channels.SingleOrDefault(c => c.FundingTransactionId == fundingTxId &&
                                                                        c.FundingOutputIndex == fundingTxOutputIndex);

            if (channel == null)
            {
                return;
            }
            
            if (spendingTx.Inputs[0].Sequence == 0xFFFFFFFF)
            {
                OnMutualCloseTransaction(channel, spendingTx);
                return;
            }
            
            var transactionNumber = TransactionNumber.CalculateNumber(spendingTx.Inputs[0].Sequence, 
                channel.LocalCommitmentTxParameters.PaymentBasepoint, 
                channel.RemoteCommitmentTxParameters.PaymentBasepoint);

            if (transactionNumber == channel.RemoteCommitmentTxParameters.TransactionNumber)
            {
                OnCommitmentTransaction(channel, spendingTx);
                return;
            }

            if (transactionNumber < channel.RemoteCommitmentTxParameters.TransactionNumber)
            {
                OnRevokedCommitmentTransaction(channel, spendingTx);
                return;
            }

            if (transactionNumber > channel.RemoteCommitmentTxParameters.TransactionNumber)
            {
                OnNewerCommitmentTransaction(channel, spendingTx);
            }
        }

        private void OnMutualCloseTransaction(LocalChannel channel, Transaction spendingTx)
        {
            var oldState = channel.State;
            channel.State = LocalChannelState.Closed;
            _channelLoggingService.LogStateUpdate(channel, oldState, "Mutual Close: Closing Transaction confirmed. ");
            _channelService.RemoveChannel(channel);
        }

        private void OnCommitmentTransaction(LocalChannel channel, Transaction spendingTx)
        {
            var oldState = channel.State;
            _channelLoggingService.LogWarning(channel, "Unilateral Close: A commitment transaction got confirmed.", spendingTx.ToString());
            _unilateralCloseService.HandleUnilateralClose(channel, spendingTx);
            channel.State = LocalChannelState.Closed;
            _channelLoggingService.LogStateUpdate(channel, oldState);
            _channelService.RemoveChannel(channel);
        }

        private void OnRevokedCommitmentTransaction(LocalChannel channel, Transaction spendingTx)
        {
            var oldState = channel.State;
            _channelLoggingService.LogWarning(channel, "Penalty: A revoked commitment tx got confirmed.", spendingTx.ToString());
            _penaltyService.HandlePenalty(channel, spendingTx);
            channel.State = LocalChannelState.Penalty;
            _channelLoggingService.LogStateUpdate(channel, oldState);
            _channelService.RemoveChannel(channel);
        }
        
        private void OnNewerCommitmentTransaction(LocalChannel channel, Transaction spendingTx)
        {
            var oldState = channel.State;
            _channelLoggingService.LogWarning(channel, "A newer commitment tx got confirmed.", spendingTx.ToString());
            _unilateralCloseService.HandleNewerCommitmentTransaction(channel, spendingTx);
            channel.State = LocalChannelState.Closed;
            _channelLoggingService.LogStateUpdate(channel, oldState);
            _channelService.RemoveChannel(channel);
        }        
        
        private void MonitorChannels()
        {
            foreach (var channel in _channelService.Channels)
            {
                MonitorChannel(channel);
            }
        }

        private void MonitorChannel(LocalChannel channel)
        {
            _blockchainMonitorService.WatchForSpendingTransaction(channel.FundingTransactionId, channel.FundingOutputIndex);
        }

        public void Dispose()
        {
            _taskScheduler.Dispose();
        }
    }
}