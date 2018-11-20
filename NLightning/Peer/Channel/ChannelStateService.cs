using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using NLightning.Peer.Channel.Establishment;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Models;
using NLightning.Transport.Messaging;

namespace NLightning.Peer.Channel
{
    public class ChannelStateService : IChannelStateService, IDisposable
    {
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly IChannelService _channelService;
        private readonly IPeerService _peerService;
        private readonly IChannelLoggingService _channelLoggingService;
        private readonly IChannelEstablishmentService _channelEstablishmentService;
        private readonly IChannelReestablishmentService _channelReestablishmentService;
        private readonly IChannelCloseService _channelCloseService;
        private readonly EventLoopScheduler _taskScheduler = new EventLoopScheduler();
        private readonly ILogger _logger;
        private readonly Subject<LocalChannel> _activeStateProvider = new Subject<LocalChannel>();
        
        public ChannelStateService(ILoggerFactory loggerFactory, IChannelService channelService, 
                                    IPeerService peerService, IChannelLoggingService channelLoggingService, 
                                    IChannelEstablishmentService channelEstablishmentService,
                                    IChannelReestablishmentService channelReestablishmentService,
                                    IChannelCloseService channelCloseService)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _channelService = channelService;
            _peerService = peerService;
            _channelLoggingService = channelLoggingService;
            _channelEstablishmentService = channelEstablishmentService;
            _channelReestablishmentService = channelReestablishmentService;
            _channelCloseService = channelCloseService;
        }

        public IObservable<LocalChannel> ChannelActiveStateChangedProvider => _activeStateProvider;

        public void Initialize()
        {
            _subscriptions.Add(_peerService.MessagingStateProvider
                .ObserveOn(_taskScheduler)
                .Where(m => m.Item2 == MessagingClientState.Stopped)
                .Subscribe(peerMessage => OnPeerDisconnected(peerMessage.Peer)));
            
            _subscriptions.Add(_channelEstablishmentService.SuccessProvider
                .ObserveOn(_taskScheduler)
                .Subscribe(peerMessage => OnSuccessfulEstablishment(peerMessage.Channel)));
            
            _subscriptions.Add(_channelCloseService.ChannelClosingProvider
                .ObserveOn(_taskScheduler)
                .Subscribe(OnChannelClosing));
            
            _subscriptions.Add(_channelReestablishmentService.ChannelReestablishedProvider
                .ObserveOn(_taskScheduler)
                .Subscribe(peerMessage => OnSuccessfulEstablishment(peerMessage.Channel)));
        }

        private void OnChannelClosing(LocalChannel channel)
        {
            channel.Active = false;
            ChannelActiveStateChanged(channel);
        }
        
        private void OnSuccessfulEstablishment(LocalChannel channel)
        {
            if (channel.State == LocalChannelState.NormalOperation)
            {
                channel.Active = true;
                ChannelActiveStateChanged(channel);
            }
        }
        
        private void ChannelActiveStateChanged(LocalChannel channel)
        {
            _activeStateProvider.OnNext(channel);
        }

        private void OnPeerDisconnected(IPeer peer)
        {
            var channel = _channelService.Channels.Where(c => c.PersistentPeer.Address == peer.NodeAddress.Address).ToList();
            
            channel.ForEach(c =>
            {
                c.Active = false;
                _channelLoggingService.LogInfo(c, $"Peer {peer.NodeAddress} disconnected. Channel inactive.");
            });
        }

        public void Dispose()
        {
            _subscriptions.ForEach(s => s.Dispose());
            _taskScheduler.Dispose();
            _activeStateProvider.Dispose();
        }
    }
}