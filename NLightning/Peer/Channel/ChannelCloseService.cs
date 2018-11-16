using System;
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
using NLightning.Peer.Channel.ChannelCloseMessages;
using NLightning.Peer.Channel.Configuration;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Logging.Models;
using NLightning.Peer.Channel.Models;
using NLightning.Transport.Messaging.SetupMessages;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using NLightning.Wallet;
using NLightning.Wallet.Commitment;

namespace NLightning.Peer.Channel
{
    public class ChannelCloseService : IChannelCloseService, IDisposable
    {
        private readonly EventLoopScheduler _taskScheduler = new EventLoopScheduler();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly IChannelService _channelService;
        private readonly IPeerService _peerService;
        private readonly IWalletService _walletService;
        private readonly IBlockchainClientService _blockchainClientService;
        private readonly IChannelLoggingService _channelLoggingService;
        private readonly ILogger _logger;
        private readonly ChannelConfiguration _configuration;
        private readonly List<LocalChannel> _failedFeeNegotiations = new List<LocalChannel>();
        private readonly Subject<LocalChannel> _channelClosingProvider = new Subject<LocalChannel>();
        private readonly Subject<LocalChannel> _channelClosedProvider = new Subject<LocalChannel>();
        private NetworkParameters _networkParameters;

        public ChannelCloseService(ILoggerFactory loggerFactory, IChannelService channelService, IPeerService peerService, 
                                    IWalletService walletService, IConfiguration configuration, IBlockchainClientService blockchainClientService, 
                                    IChannelLoggingService channelLoggingService)
        {
            _channelService = channelService;
            _peerService = peerService;
            _walletService = walletService;
            _blockchainClientService = blockchainClientService;
            _channelLoggingService = channelLoggingService;
            _configuration = configuration.GetConfiguration<ChannelConfiguration>();
            _logger = loggerFactory.CreateLogger(GetType());
        }
       
        public void Initialize(NetworkParameters networkParameters)
        {
            _networkParameters = networkParameters;
            SubscribeToEvents();
        }

        public IObservable<LocalChannel> ChannelClosingProvider => _channelClosingProvider;
        public IObservable<LocalChannel> ChannelClosedProvider => _channelClosedProvider;

        public void Close(LocalChannel channel, bool unilateralCloseOnUnavailability)
        {
            _channelLoggingService.LogInfo(channel, $"Closing {channel.ChannelId}. State: {channel.State}. Force: {unilateralCloseOnUnavailability}");
            var peer = _peerService.Peers.SingleOrDefault(p => p.NodeAddress.Address == channel.PersistentPeer.Address);
            if (peer == null && unilateralCloseOnUnavailability)
            {
                _logger.LogWarning($"Peer ({channel.PersistentPeer.Address}) is not available. Doing an unilateral close.");
                UnilateralClose(channel);
                return;
            }

            if (peer == null)
            {
                throw new PeerException("Peer is unavailable");
            }
            
            MutualClose(channel, peer);
        }

        private void MutualClose(LocalChannel channel, IPeer peer)
        {
            var oldState = channel.State;

            if (channel.State != LocalChannelState.ClosingSigned)
            {
                channel.State = LocalChannelState.Shutdown;
            }

            channel.CloseReason = CloseReason.LocalMutualClose;
            
            SendShutdownMessage(channel, peer);

            _channelService.UpdateChannel(channel);
            _channelLoggingService.LogStateUpdate(channel, oldState, "Mutual Close");
            _channelClosingProvider.OnNext(channel);
        }

        private void SendShutdownMessage(LocalChannel channel, IPeer peer)
        {
            ShutdownMessage shutdownMessage = new ShutdownMessage();
            shutdownMessage.ChannelId = channel.ChannelId.HexToByteArray();
            shutdownMessage.ScriptPubKey = _walletService.PubKeyAddress.ScriptPubKey.ToBytes(); //channel.FinalPubKeyScript;
            peer.Messaging.Send(shutdownMessage);
        }

        public void UnilateralClose(LocalChannel channel)
        {
            var oldState = channel.State;
            _channelLoggingService.LogInfo(channel, $"Local UnilateralClose {channel.ChannelId}. State: {channel.State}.");
            channel.State = LocalChannelState.LocalUnilateralClose;

            if (channel.CloseReason == CloseReason.None)
            {
                channel.CloseReason = CloseReason.LocalUnilateralClose;
            }
            
            var builder = new CommitmentTransactionBuilder(channel, true, _networkParameters);
            var commitmentTx = builder.BuildWithSignatures();
            
            _channelLoggingService.LogInfo(channel, "Unilateral Close Transaction", commitmentTx.ToString());
            _blockchainClientService.SendTransaction(commitmentTx);
            
            _channelService.UpdateChannel(channel);
            _channelLoggingService.LogStateUpdate(channel, oldState, "Unilateral Close");
            _channelClosingProvider.OnNext(channel);
        }
        
        public void ShutdownUnknownChannel(IPeer peer, byte[] channelId)
        {
            peer.Messaging.Send(new ShutdownMessage
            {
                ChannelId = channelId,
                ScriptPubKey = _walletService.PubKeyAddress.ScriptPubKey.ToBytes()
            });
        }

        private void SubscribeToEvents()
        {
            _subscriptions.Add(_peerService.IncomingMessageProvider
                .ObserveOn(_taskScheduler)
                .Where(m => m.Item2 is ShutdownMessage)
                .Subscribe(peerMessage => OnShutdownMessage(peerMessage.Item1, (ShutdownMessage) peerMessage.Item2)));
            
            _subscriptions.Add(_peerService.IncomingMessageProvider
                .ObserveOn(_taskScheduler)
                .Where(m => m.Item2 is ClosingSignedMessage)
                .Subscribe(peerMessage => OnClosingSigned(peerMessage.Item1, (ClosingSignedMessage) peerMessage.Item2)));
        }

        private void OnClosingSigned(IPeer peer, ClosingSignedMessage message)
        {
            var channel = _channelService.Channels.SingleOrDefault(c => c.ChannelId == message.ChannelId.ToHex());
            if (channel == null)
            {
                _logger.LogDebug($"Remote sent us a {nameof(ClosingSignedMessage)}, but there is no matching channel.");
                peer.Messaging.Send(ErrorMessage.UnknownChannel(message.ChannelId));
                return;
            }

            // TODO: verify signature

            var signedCloseTx = BuildSignedCloseTransaction(channel, SignatureConverter.RawToTransactionSignature(message.Signature), message.FeeSatoshi);
            if (channel.IsFunder)
            {
                bool isFairFee = IsFairFee(signedCloseTx, message.FeeSatoshi);
                if (isFairFee)
                {
                    _channelLoggingService.LogInfo(channel, "Mutual Close Transaction", signedCloseTx.ToString());
                    _blockchainClientService.SendTransaction(signedCloseTx);
                }
                else if (_failedFeeNegotiations.Contains(channel))
                {
                    _logger.LogWarning("Unable to negotiate a fair fee for our closing transaction. Will do a unilateral close.");
                    UnilateralClose(channel);
                }
                else
                {
                    _failedFeeNegotiations.Add(channel);
                    RespondWithClosingSigned(peer, channel);
                }
            }
            else
            {
                RespondWithClosingSigned(peer, channel, message.FeeSatoshi);
            }
            
            _channelService.UpdateChannel(channel);
        }

        private bool IsFairFee(Transaction signedCloseTx, ulong theirFee)
        {
            var deviationMax = _configuration.ClosingFeeDeviationMaximumPercentage;
            ulong ourFeeRate = _blockchainClientService.GetFeeRatePerKw(3);
            var size = signedCloseTx.GetSerializedSize(2, SerializationType.Network);
            var fee = TransactionFee.CalculateFee(ourFeeRate, (ulong)size);
            
            return theirFee < fee + fee * deviationMax && 
                   theirFee > fee - fee * deviationMax;
        }

        private Transaction BuildSignedCloseTransaction(LocalChannel channel, TransactionSignature remoteClosingSignature, ulong feeSatoshi)
        {
            var builder = new CloseChannelTransactionBuilder(channel, _networkParameters);
            builder.FeeSatoshi = feeSatoshi;
            return builder.BuildWithSignatures(remoteClosingSignature);
        }

        private void OnShutdownMessage(IPeer peer, ShutdownMessage message)
        {
            var channel = _channelService.Channels.SingleOrDefault(c => c.ChannelId == message.ChannelId.ToHex());
            if (channel == null)
            {
                _logger.LogDebug($"Remote sent us a {nameof(ShutdownMessage)}, but there is no matching channel.");
                peer.Messaging.Send(ErrorMessage.UnknownChannel(message.ChannelId));
                return;
            }

            if (channel.RemoteChannelParameters.ShutdownScriptPubKey != null &&
                !channel.RemoteChannelParameters.ShutdownScriptPubKey.SequenceEqual(message.ScriptPubKey))
            {
                _channelLoggingService.LogError(channel, LocalChannelError.InvalidShutdownScriptPubKey,  "Received shutdown message with invalid ShutdownScriptPubKey. Will do an unilateral close");
                channel.CloseReason = CloseReason.InvalidShutdownPubKey;
                UnilateralClose(channel);
                return;
            }
            
            channel.RemoteChannelParameters.ShutdownScriptPubKey = message.ScriptPubKey;

            if (channel.State == LocalChannelState.NormalOperation)
            {
                channel.CloseReason = CloseReason.RemoteMutualClose;
                RespondWithShutdown(peer, channel, message);
            }
            else if (channel.State == LocalChannelState.Shutdown || 
                     channel.State == LocalChannelState.ClosingSigned)
            {
                RespondWithClosingSigned(peer, channel);
            }
            
            _channelService.UpdateChannel(channel);
        }

        private void RespondWithShutdown(IPeer peer, LocalChannel channel, ShutdownMessage message)
        {
            ShutdownMessage shutdownMessage = new ShutdownMessage();
            shutdownMessage.ScriptPubKey = channel.LocalChannelParameters.ShutdownScriptPubKey;
            shutdownMessage.ChannelId = channel.ChannelId.HexToByteArray();
            peer.Messaging.Send(shutdownMessage);
        }

        private void RespondWithClosingSigned(IPeer peer, LocalChannel channel, ulong fee = 0)
        {
            var oldState = channel.State;
            ulong ourFeeRate = _blockchainClientService.GetFeeRatePerKw(3);
            channel.State = LocalChannelState.ClosingSigned;

            var builder = new CloseChannelTransactionBuilder(channel, _networkParameters);
            builder.FeeSatoshi = 0;
            var zeroFeeTx = builder.Build();
            builder.FeeSatoshi = fee != 0 ? fee : TransactionFee.CalculateFee(ourFeeRate, (ulong)zeroFeeTx.GetSerializedSize(2, SerializationType.Network));
            
            ClosingSignedMessage closingSignedMessage = new ClosingSignedMessage();
            closingSignedMessage.FeeSatoshi = builder.FeeSatoshi;
            closingSignedMessage.ChannelId = channel.ChannelId.HexToByteArray();
            closingSignedMessage.Signature = builder.Sign().ToRawSignature();
            
            peer.Messaging.Send(closingSignedMessage);
            _channelLoggingService.LogStateUpdate(channel, oldState, "Respond with closing signed");
        }

        public void Dispose()
        {
            _taskScheduler.Dispose();
            _subscriptions.ForEach(s => s.Dispose());
            _channelClosedProvider.Dispose();
            _channelClosingProvider.Dispose();
        }
    }
}