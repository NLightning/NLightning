using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NLightning.Network;
using NLightning.OnChain.Client;
using NLightning.OnChain.Monitoring;
using NLightning.Peer.Channel.Configuration;
using NLightning.Peer.Channel.Establishment.Messages;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Logging.Models;
using NLightning.Peer.Channel.Models;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.SetupMessages;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using NLightning.Wallet;
using NLightning.Wallet.Commitment;
using NLightning.Wallet.Funding;
using NLightning.Wallet.KeyDerivation;

namespace NLightning.Peer.Channel.Establishment
{
    public class ChannelEstablishmentService : IChannelEstablishmentService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly IPeerService _peerService;
        private readonly IFundingService _fundingService;
        private readonly IChannelLoggingService _channelLoggingService;
        private readonly ICommitmentTransactionService _commitmentService;
        private readonly IChannelService _channelService;
        private readonly IBlockchainClientService _blockchainClientService;
        private readonly IKeyDerivationService _keyDerivationService;
        private readonly IBlockchainMonitorService _blockchainMonitorService;
        private readonly IWalletService _walletService;
        private readonly ChannelConfiguration _configuration;
        private readonly EventLoopScheduler _taskScheduler = new EventLoopScheduler();
        private readonly Subject<(IPeer, LocalChannel)> _successProvider = new Subject<(IPeer, LocalChannel)>();
        private readonly Subject<(IPeer, PendingChannel, string)> _failedProvider = new Subject<(IPeer, PendingChannel, string)>();
        private readonly AcceptChannelMessageHandler _acceptChannelMessageHandler;
        private readonly FundingMessageSignedHandler _fundingMessageSignedHandler;
        private readonly FundingMessageLockedHandler _fundingMessageLockedHandler;
        private NetworkParameters _networkParameters;

        public ChannelEstablishmentService(ILoggerFactory loggerFactory, IConfiguration configuration,
            IPeerService peerService, IFundingService fundingService, IChannelLoggingService channelLoggingService,
            ICommitmentTransactionService commitmentService, IChannelService channelService, IBlockchainClientService blockchainClientService,
            IKeyDerivationService keyDerivationService, IBlockchainMonitorService blockchainMonitorService, IWalletService walletService)
        {
            _peerService = peerService;
            _fundingService = fundingService;
            _channelLoggingService = channelLoggingService;
            _commitmentService = commitmentService;
            _channelService = channelService;
            _blockchainClientService = blockchainClientService;
            _keyDerivationService = keyDerivationService;
            _blockchainMonitorService = blockchainMonitorService;
            _walletService = walletService;
            _logger = loggerFactory.CreateLogger(GetType());
            _configuration = configuration.GetConfiguration<ChannelConfiguration>();
            _acceptChannelMessageHandler = new AcceptChannelMessageHandler(_channelLoggingService, _fundingService, _commitmentService);
            _fundingMessageSignedHandler = new FundingMessageSignedHandler(_channelLoggingService, _fundingService, _commitmentService, _channelService, _blockchainMonitorService);
            _fundingMessageLockedHandler = new FundingMessageLockedHandler();
        }

        public IObservable<(IPeer, PendingChannel, string)> FailureProvider => _failedProvider;
        public IObservable<(IPeer, LocalChannel)> SuccessProvider => _successProvider;
        
        public void Initialize(NetworkParameters networkParameters)
        {
            _networkParameters = networkParameters;
            SubscribeToEvents();
            WatchForFundingTransactions();
        }

        private void WatchForFundingTransactions()
        {
            var pendingChannels = _channelService.Channels.Where(c => c.State == LocalChannelState.FundingSigned || 
                                                                      c.State == LocalChannelState.FundingLocked).ToList();
            
            foreach (var pendingChannel in pendingChannels)
            {
                WatchForFundingTransaction(pendingChannel);
            }
        }

        private void WatchForFundingTransaction(LocalChannel channel)
        {
            _blockchainMonitorService.WatchForTransactionId(channel.FundingTransactionId, (ushort)channel.MinimumDepth);
        }

        private void SubscribeToEvents()
        {
            _subscriptions.Add(_peerService.IncomingMessageProvider
                .ObserveOn(_taskScheduler)
                .Where(m => m.Message is AcceptChannelMessage)
                .Subscribe(peerMessage =>
                    OnAcceptChannelMessage(peerMessage.Peer, (AcceptChannelMessage) peerMessage.Message)));
            
            _subscriptions.Add(_peerService.IncomingMessageProvider
                .ObserveOn(_taskScheduler)
                .Where(m => m.Message is FundingSignedMessage)
                .Subscribe(peerMessage =>
                    OnFundingSignedMessage(peerMessage.Peer, (FundingSignedMessage) peerMessage.Message)));
            
            _subscriptions.Add(_peerService.IncomingMessageProvider
                .ObserveOn(_taskScheduler)
                .Where(m => m.Message is FundingLockedMessage)
                .Subscribe(peerMessage =>
                    OnFundingLockedMessage(peerMessage.Peer, (FundingLockedMessage) peerMessage.Message)));
            
            _subscriptions.Add(_peerService.ValidationExceptionProvider
                .ObserveOn(_taskScheduler)
                .Subscribe(peerMessage =>
                    OnValidationException(peerMessage.Peer, peerMessage.ValidationException)));
            
            _subscriptions.Add(_blockchainMonitorService.ByTransactionIdProvider
                .ObserveOn(_taskScheduler)
                .Subscribe(OnTransactionConfirmed));
        }

        private void OnAcceptChannelMessage(IPeer peer, AcceptChannelMessage acceptMessage)
        {
            var pendingChannel = _channelService.PendingChannels.SingleOrDefault(est => est.Peer == peer &&
                                          est.OpenMessage.TemporaryChannelId.SequenceEqual(acceptMessage.TemporaryChannelId));
            if (pendingChannel == null)
            {
                _logger.LogDebug($"Remote sent us an {nameof(AcceptChannelMessage)}, but there are no matching pending channel open messages.");
                peer.Messaging.Send(ErrorMessage.UnknownChannel(acceptMessage.TemporaryChannelId));
                return;
            }
            
            var fundingCreatedMessage = _acceptChannelMessageHandler.Handle(acceptMessage, pendingChannel);
            peer.Messaging.Send(fundingCreatedMessage);
        }

        public PendingChannel OpenChannel(IPeer peer, ulong fundingSatoshis, ulong pushMSat = 0)
        {
            OpenChannelMessage message = new OpenChannelMessage();

            message.ChainHash = _networkParameters.ChainHash;
            message.FundingSatoshis = fundingSatoshis;
            message.TemporaryChannelId = GenerateTemporaryChannelId();
            message.ChannelFlags = CreateChannelFlags();
            message.PushMSat = pushMSat;
            message.ToSelfDelay = _configuration.ToSelfDelay;

            message.DustLimitSatoshis = _configuration.DustLimit;
            message.FeeratePerKw = GetFeeRatePerKw();
            message.MaxAcceptedHtlcs = (ushort) _configuration.AcceptHtlcMax;
            message.HtlcMinimumMSat = _configuration.HtlcMinMSat;
            message.MaxHtlcValueInFlightMSat = _configuration.HtlcInFlightMSat;
            message.ChannelReserveSatoshis = Math.Max((ulong) (_configuration.ReserveToFundingRatio * fundingSatoshis),  _configuration.DustLimit);

            uint channelIndex = _channelService.GetNextChannelIndex();

            message.HtlcBasepoint = _keyDerivationService.DeriveKey(KeyFamily.HtlcBase, channelIndex);
            message.PaymentBasepoint = _keyDerivationService.DeriveKey(KeyFamily.PaymentBase, channelIndex);
            message.RevocationBasepoint = _keyDerivationService.DeriveKey(KeyFamily.RevocationBase, channelIndex);
            message.DelayedPaymentBasepoint = _keyDerivationService.DeriveKey(KeyFamily.DelayBase, channelIndex);
            message.FundingPubKey = _keyDerivationService.DeriveKey(KeyFamily.MultiSig, channelIndex);
            var revocationKey = _keyDerivationService.DeriveKey(KeyFamily.RevocationRoot, channelIndex);
            message.FirstPerCommitmentPoint = _keyDerivationService.DerivePerCommitmentPoint(revocationKey, 0);
            message.ShutdownScriptPubKey = _walletService.ShutdownScriptPubKey;
            
            var establishment = new PendingChannel(peer, message, revocationKey, channelIndex);
            _channelService.AddPendingChannel(establishment);

            peer.Messaging.Send(message);
            
            _channelLoggingService.LogPendingChannelInfo(message.TemporaryChannelId.ToHex(), LocalChannelState.OpenChannel, 
                $"Open a channel with {peer.NodeAddress}. Amount: {fundingSatoshis} Satoshi. PushMSat: {pushMSat}");
            return establishment;
        }

        private void OnFundingSignedMessage(IPeer peer, FundingSignedMessage message)
        {
            var pendingChannel = _channelService.PendingChannels.SingleOrDefault(est => est.Peer == peer && est.Channel != null && est.Channel.ChannelId == message.ChannelId.ToHex());
            if (pendingChannel == null)
            {
                _logger.LogError($"Remote sent us an {nameof(FundingSignedMessage)}, but there are no matching pending channels.");
                peer.Messaging.Send(ErrorMessage.UnknownChannel(message.ChannelId));
                return;
            }

            try
            {
                _fundingMessageSignedHandler.Handle(peer, message, pendingChannel);
            }
            catch (ChannelException exception)
            {
                ChannelEstablishmentFailed(peer, pendingChannel, exception.Message);
            }
        }
        
        private void OnTransactionConfirmed(Transaction transaction)
        {
            var channel = _channelService.Channels.SingleOrDefault(c => (c.State == LocalChannelState.FundingSigned || c.State == LocalChannelState.FundingLocked)  &&
                                                                          c.FundingTransactionId == transaction.GetHash().ToString());
            if (channel == null) return;
            _channelLoggingService.LogInfo(channel, "Funding Transaction confirmed");
            var peer = _peerService.Peers.SingleOrDefault(p => p.NodeAddress.Address == channel.PersistentPeer.Address);
            if (peer == null)
            {
                ScheduleTransactionConfirmed(transaction, channel);
                return;
            }
            
            LocalFundingLocked(peer, channel);
        }

        private void ScheduleTransactionConfirmed(Transaction transaction, LocalChannel channel)
        {
            _channelLoggingService.LogWarning(channel, "Funding Locked was scheduled: Peer not connected.");
            
            IDisposable subscription = null;
            subscription = _taskScheduler.Schedule(TimeSpan.FromMinutes(5), () =>
            {
                OnTransactionConfirmed(transaction);
                _subscriptions.Remove(subscription);
            });
            
            _subscriptions.Add(subscription);
            _logger.LogWarning($"Can't move channel {channel.ChannelId} to normal operation: Peer is not connected. Will try again in 5 minutes.");
        }

        private void LocalFundingLocked(IPeer peer, LocalChannel channel)
        {
            var oldState = channel.State;
            
            if (channel.State != LocalChannelState.FundingSigned && channel.State != LocalChannelState.FundingLocked)
            {
                _channelLoggingService.LogError(channel, LocalChannelError.InvalidState, $"Can't lock funding. Current state is: {channel.State}");
                return;
            }
            
            channel.State = channel.State == LocalChannelState.FundingLocked ? LocalChannelState.NormalOperation : LocalChannelState.FundingLocked;
            channel.LocalCommitmentTxParameters.NextPerCommitmentPoint = _commitmentService.GetNextLocalPerCommitmentPoint(channel);
            
            _channelService.UpdateChannel(channel);
            SendFundingLocked(peer, channel);

            if (channel.State == LocalChannelState.NormalOperation)
            {
                ChannelEstablishmentSuccessful(peer, channel);
            }
            
            _channelLoggingService.LogStateUpdate(channel, oldState);
        }

        private void SendFundingLocked(IPeer peer, LocalChannel channel)
        {
            FundingLockedMessage message = new FundingLockedMessage();
            message.ChannelId = channel.ChannelId.HexToByteArray();
            message.NextPerCommitmentPoint = _commitmentService.GetNextLocalPerCommitmentPoint(channel);
            peer.Messaging.Send(message);
        }

        private void ChannelEstablishmentSuccessful(IPeer peer, LocalChannel channel)
        {
            _successProvider.OnNext((peer, channel));
        }
        
        private void ChannelEstablishmentFailed(IPeer peer, PendingChannel pendingChannel, string errorMessage)
        {
            _channelService.RemovePendingChannel(pendingChannel);
            _failedProvider.OnNext((peer, pendingChannel, errorMessage));
        }
        
        private void OnValidationException(IPeer peer, MessageValidationException exception)
        {
            if (exception.FailChannelId == null)
            {
                return;
            }

            var channelId = exception.FailChannelId.ToHex();
            var pendingChannel = _channelService.PendingChannels.SingleOrDefault(est => est.Peer == peer && est.TemporaryChannelId == channelId);
            if (pendingChannel != null)
            {
                ChannelEstablishmentFailed(peer, pendingChannel, exception.Message);
            }
        }

        private void OnFundingLockedMessage(IPeer peer, FundingLockedMessage message)
        {
            var channel = _channelService.Channels.SingleOrDefault(c => c.ChannelId == message.ChannelId.ToHex());
            if (channel == null)
            {
                _logger.LogError($"Remote sent us a {nameof(FundingLockedMessage)}, but there are no matching pending channels.");
                peer.Messaging.Send(ErrorMessage.UnknownChannel(message.ChannelId));
                return;
            }

            if (channel.State == LocalChannelState.NormalOperation && channel.RemoteCommitmentTxParameters.TransactionNumber == 0)
            {
                _channelLoggingService.LogInfo(channel, $"Remote sent us a {nameof(FundingLockedMessage)} but we are already in Normal Operation state. " +
                                                        "We will answer with a funding locked message.");
                SendFundingLocked(peer, channel);
                return;
            }
            
            if (channel.State != LocalChannelState.FundingSigned && channel.State != LocalChannelState.FundingLocked)
            {
                _channelLoggingService.LogWarning(channel, $"Remote sent us a {nameof(FundingLockedMessage)}, but the current state is {channel.State}");
                return;
            }
            
            var oldState = channel.State;

            _fundingMessageLockedHandler.Handle(message, channel);
            _channelService.UpdateChannel(channel);
            
            if (channel.State == LocalChannelState.NormalOperation)
            {
                ChannelEstablishmentSuccessful(peer, channel);
            }
            
            _channelLoggingService.LogStateUpdate(channel, oldState);
        }

        private uint GetFeeRatePerKw()
        {
            var feeRate = _blockchainClientService.GetFeeRatePerKw(3);
            return Math.Max(feeRate, _configuration.FeePerKwMinimum);
        }

        private byte CreateChannelFlags()
        {
            byte[] flags = {0};
            BitArray bitArray = new BitArray(flags);
            bitArray.Set(0, _configuration.AnnounceChannels);
            bitArray.CopyTo(flags, 0);
            return flags[0];
        }

        private static byte[] GenerateTemporaryChannelId()
        {
            byte[] temporaryChannelId = new byte[32];
            Random r = new Random();
            r.NextBytes(temporaryChannelId);
            return temporaryChannelId;
        }

        public void Dispose()
        {
            _subscriptions.ForEach(s => s.Dispose());
            _taskScheduler?.Dispose();
            _failedProvider.Dispose();
            _successProvider.Dispose();
        }
    }
}