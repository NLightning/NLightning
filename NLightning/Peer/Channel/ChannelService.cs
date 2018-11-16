using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLightning.Network;
using NLightning.OnChain;
using NLightning.Peer.Channel.Models;
using NLightning.Persistence;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel
{
    public class ChannelService : IChannelService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly LocalPersistenceContext _dbContext;
        private readonly List<LocalChannel> _channels = new List<LocalChannel>();
        private readonly List<PendingChannel> _pendingChannels = new List<PendingChannel>();
        private readonly Subject<LocalChannel> _channelAddedProvider = new Subject<LocalChannel>();
        private readonly Subject<LocalChannel> _channelRemovedProvider = new Subject<LocalChannel>();
        private readonly EventLoopScheduler _taskScheduler = new EventLoopScheduler();
        private readonly object _syncLock = new object();
        private uint ChannelIndex;
        
        public ChannelService(ILoggerFactory loggerFactory, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _dbContext = scopeFactory.CreateScopedService<LocalPersistenceContext>();
        }

        public ReadOnlyCollection<PendingChannel> PendingChannels
        {
            get
            {
                lock (_syncLock)
                {
                    return _pendingChannels.AsReadOnly();
                }
            }
        }

        public ReadOnlyCollection<LocalChannel> Channels
        {
            get
            {
                lock (_syncLock)
                {
                    return _channels.AsReadOnly();
                }
            }
        }
        
        public IObservable<LocalChannel> ChannelAddedProvider => _channelAddedProvider;
        public IObservable<LocalChannel> ChannelRemovedProvider => _channelRemovedProvider;

        public LocalChannel Find(string channelId)
        {
            lock(_syncLock)
            {
                return _channels.SingleOrDefault(c => c.ChannelId == channelId);
            }
        }

        public void Initialize(NetworkParameters networkParameters)
        {
            LoadChannels();
        }

        private void LoadChannels()
        {
            lock (_syncLock)
            {
                _channels.Clear();
                _channels.AddRange(_dbContext.LocalChannels
                    .Include(channel => channel.PersistentPeer)
                    .Include(channel => channel.LocalCommitmentTxParameters)
                    .Include(channel => channel.RemoteCommitmentTxParameters)
                    .Include(channel => channel.LocalChannelParameters)
                    .Include(channel => channel.RemoteChannelParameters)
                    .Include(channel => channel.Htlcs)
                    .Where(c => c.State != LocalChannelState.Closed &&
                                c.State != LocalChannelState.FundingFailed));
                _logger.LogInformation($"Loaded {_channels.Count} channels.");
            }
        }

        public void AddChannel(IPeer peer, LocalChannel channel)
        {
            lock (_syncLock)
            {
                _logger.LogInformation($"Add Channel {channel.ChannelId}");
                channel.PersistentPeer = _dbContext.Peers.SingleOrDefault(p => p.Address == peer.NodeAddress.Address) ?? new PersistentPeer(peer.NodeAddress.Address, true);
                _dbContext.Add(channel);
                _dbContext.SaveChanges();
                _channels.Add(channel);
                _channelAddedProvider.NotifyOn(_taskScheduler).OnNext(channel);
            }
        }

        public void UpdateChannel(LocalChannel channel)
        {
            lock (_syncLock)
            {
                _dbContext.SaveChanges();
            }
        }

        public void RemoveChannel(LocalChannel channel)
        {
            if (channel.State != LocalChannelState.Closed)
            {
                throw new ChannelException("Channel needs to be closed. Use ChannelCloseService to remove a channel.", channel);
            }
            
            lock (_syncLock)
            {
                _channels.Remove(channel);
                _dbContext.SaveChanges();
                _channelRemovedProvider.NotifyOn(_taskScheduler).OnNext(channel);
            }
        }

        public void AddPendingChannel(PendingChannel pendingChannel)
        {
            lock(_syncLock)
            {
                _pendingChannels.Add(pendingChannel);
            }
        }

        public void RemovePendingChannel(PendingChannel pendingChannel)
        {
            lock(_syncLock)
            {
                _pendingChannels.Remove(pendingChannel);
            }
        }

        public uint GetNextChannelIndex()
        {
            lock (_syncLock)
            {
                if (ChannelIndex != 0)
                {
                    return ++ChannelIndex;
                }

                ChannelIndex = _channels.Count == 0 ? 1 : _channels.Select(m => m.ChannelIndex).Max() + 1;
                return ChannelIndex;
            }
        }

        public void Dispose()
        {
            _taskScheduler.Dispose();
            _dbContext?.Dispose();
            _channelAddedProvider.Dispose();
            _channelRemovedProvider.Dispose();
        }
    }
}