using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using NLightning.Peer.Channel.ChannelCloseMessages;
using NLightning.Peer.Channel.ChannelEstablishmentMessages;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Models;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.SetupMessages;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel
{
    public class ChannelReestablishmentService : IChannelReestablishmentService, IDisposable
    {
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly IChannelService _channelService;
        private readonly IPeerService _peerService;
        private readonly IChannelCloseService _channelCloseService;
        private readonly IChannelLoggingService _channelLoggingService;
        private readonly EventLoopScheduler _taskScheduler = new EventLoopScheduler();
        private readonly ILogger _logger;
        private readonly Subject<(IPeer, LocalChannel)> _channelReestablishedProvider = new Subject<(IPeer, LocalChannel)>();

        public ChannelReestablishmentService(ILoggerFactory loggerFactory, IChannelService channelService, 
                                             IPeerService peerService, IChannelCloseService channelCloseService, 
                                             IChannelLoggingService channelLoggingService)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _channelService = channelService;
            _peerService = peerService;
            _channelCloseService = channelCloseService;
            _channelLoggingService = channelLoggingService;
        }
        
        public IObservable<(IPeer, LocalChannel)> ChannelReestablishedProvider => _channelReestablishedProvider;
        
        public void Initialize()
        {
            _subscriptions.Add(_peerService.IncomingMessageProvider
                .ObserveOn(_taskScheduler)
                .Where(m => m.Item2 is ChannelReestablishMessage)
                .Subscribe(peerMessage => OnReestablishMessage(peerMessage.Item1, (ChannelReestablishMessage) peerMessage.Item2)));
            
            _subscriptions.Add(_peerService.MessagingStateProvider
                .ObserveOn(_taskScheduler)
                .Where(m => m.Item2 == MessagingClientState.Active)
                .Subscribe(peerMessage => OnPeerConnected(peerMessage.Item1)));
        }

        private void OnPeerConnected(IPeer peer)
        {
            var channels = _channelService.Channels
                .Where(c => c.PersistentPeer.Address == peer.NodeAddress.Address && 
                           (c.State == LocalChannelState.FundingCreated || c.State == LocalChannelState.FundingLocked || 
                            c.State == LocalChannelState.FundingSigned || c.State == LocalChannelState.NormalOperation)).ToList();
            
            foreach (var channel in channels)
            {
                ChannelReestablishMessage message = new ChannelReestablishMessage
                {
                    ChannelId = channel.ChannelId.HexToByteArray(),
                    NextLocalCommitmentNumber = channel.LocalCommitmentTxParameters.TransactionNumber + 1,
                    NextRemoteRevocationNumber = channel.RemoteCommitmentTxParameters.TransactionNumber,
                    YourLastPerCommitmentSecret = channel.RemoteCommitmentTxParameters.PerCommitmentKey,
                    MyCurrentPerCommitmentPoint = channel.LocalCommitmentTxParameters.PerCommitmentKey
                };

                _logger.LogDebug($"Channel Reestablish, Channel: {channel.ChannelId}, Peer: {peer.NodeAddress.Address}");
                peer.Messaging.Send(message);
            }
        }

        private void OnReestablishMessage(IPeer peer, ChannelReestablishMessage message)
        {
            var channel = _channelService.Channels.SingleOrDefault(c => c.ChannelId == message.ChannelId.ToHex());
            if (channel == null)
            {
                _logger.LogWarning($"Remote sent us a {nameof(ChannelReestablishMessage)}, but we don't know the channel.");
                
                peer.Messaging.Send(ErrorMessage.UnknownChannel(message.ChannelId));
                _channelCloseService.ShutdownUnknownChannel(peer, message.ChannelId);
                return;
            }

            var nextCommitmentNumber = channel.RemoteCommitmentTxParameters.TransactionNumber + 1;
            if (nextCommitmentNumber != message.NextLocalCommitmentNumber)
            {
                _logger.LogWarning($"Invalid next commitment number ({message.NextLocalCommitmentNumber}). Ours is ({nextCommitmentNumber}).");
                peer.Messaging.Send(new ErrorMessage(message.ChannelId, "Invalid next commitment number"));
                return;
            }

            _channelReestablishedProvider.OnNext((peer, channel));
        }

        public void Dispose()
        {
            _channelReestablishedProvider.Dispose();
            _taskScheduler.Dispose();
            _subscriptions.ForEach(s => s.Dispose());
        }
    }
}