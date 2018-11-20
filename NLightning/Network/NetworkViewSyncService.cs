using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLightning.Network.Configuration;
using NLightning.Network.GossipMessages;
using NLightning.Network.Models;
using NLightning.Network.QueryMessages;
using NLightning.OnChain.Client;
using NLightning.Peer;
using NLightning.Persistence;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Network
{
    public class NetworkViewSyncService : INetworkViewSyncService, IDisposable
    {
        private static readonly int ChannelSynchronisationBatchSize = 100;
        private readonly ILogger _logger;
        private readonly object _syncLock = new object();
        private readonly IPeerService _peerService;
        private readonly IBlockchainClientService _blockchainClientService;
        private readonly NetworkViewConfiguration _configuration;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly List<(IPeer, NodeAnnouncementMessage)> _pendingNodeAnnouncementMessages = new List<(IPeer, NodeAnnouncementMessage)>();
        private readonly List<(IPeer, ChannelAnnouncementMessage)> _pendingChannelAnnouncementMessages = new List<(IPeer, ChannelAnnouncementMessage)>();
        private readonly List<(IPeer, ChannelUpdateMessage)> _pendingChannelUpdateMessages = new List<(IPeer, ChannelUpdateMessage)>();
        private readonly INetworkViewService _networkViewService;
        private readonly Subject<float> _syncProgressPercentageProvider = new Subject<float>();
        private EventLoopScheduler _eventLoopScheduler;
        private NetworkPersistenceContext _dbContext;
        private NetworkView _view;
        private NetworkParameters _networkParameters;
        private NetworkSyncDetails _ongoingSynchronisation;
        
        public NetworkViewSyncService(ILoggerFactory loggerFactory, INetworkViewService networkViewService, 
                                    IPeerService peerService, IConfiguration configuration, IBlockchainClientService blockchainClientService)
        {
            _networkViewService = networkViewService;
            _dbContext = networkViewService.NetworkPersistenceContext;
            _logger = loggerFactory.CreateLogger(GetType());
            _peerService = peerService;
            _blockchainClientService = blockchainClientService;
            _configuration = configuration.GetConfiguration<NetworkViewConfiguration>();
        }

        public NetworkView View => _view;
        public bool Synchronised { get; private set; }
        public float SyncProgressPercentage { get; private set; }
        public IObservable<float> SyncProgressPercentageProvider => _syncProgressPercentageProvider;

        public void Initialize(NetworkParameters networkParameters)
        {
            if (_configuration.SynchronisationMode != SynchronisationMode.Automatic)
            {
                return;
            }

            _networkParameters = networkParameters;
            _eventLoopScheduler = new EventLoopScheduler();
            _view = _networkViewService.View;

            SubscribeToEvents();
            InitTimer();
            ResetOutdatedPeerStates();
        }

        private void ResetOutdatedPeerStates()
        {
            var statesToReset = _view.GetPeerStates().Where(p => p.Value.LastUpdated < DateTime.Now - _configuration.ChannelTimeout && p.Value.LastBlockNumber > 0);
            foreach (var peerNetworkViewState in statesToReset)
            {
                _logger.LogInformation($"Resetting block number of network view peer state. Peer: {peerNetworkViewState.Key}. This will force a full re-sync.");
                peerNetworkViewState.Value.ResetBlockNumber();
            }
        }

        private void InitTimer()
        {
            _subscriptions.Add(_eventLoopScheduler.SchedulePeriodic(_configuration.UpdateInterval, Update));
        }
        
        private void SubscribeToEvents()
        {
            _subscriptions.Add(_peerService.IncomingMessageProvider
                .ObserveOn(_eventLoopScheduler)
                .Subscribe(message =>
                {
                    if (message.Message is ChannelAnnouncementMessage channelAnnouncementMessage)
                    {
                        OnChannelAnnouncement(message.Peer, channelAnnouncementMessage);
                    }
                    
                    if (message.Message is ChannelUpdateMessage channelUpdateMessage)
                    {
                        OnChannelUpdateMessage(message.Peer, channelUpdateMessage);
                    }
                    
                    if (message.Message is NodeAnnouncementMessage nodeAnnouncementMessage)
                    {
                        OnNodeAnnouncement(message.Peer, nodeAnnouncementMessage);
                    }
                    
                    if (message.Message is ReplyQueryChannelRangeMessage replyQueryChannelRangeMessage)
                    {
                        OnReplyQueryChannelRange(message.Peer, replyQueryChannelRangeMessage);
                    }
                   
                    if (message.Message is ReplyShortChannelIdsDoneMessage replyShortChannelIdsDoneMessage)
                    {
                        OnReplyShortChannelIdsDoneMessage(message.Peer, replyShortChannelIdsDoneMessage);
                    }
                }));

            _subscriptions.Add(_peerService.MessagingStateProvider
                .Delay(TimeSpan.FromSeconds(5))
                .ObserveOn(_eventLoopScheduler)
                .Where(tuple => tuple.Item2 == MessagingClientState.Active)
                .Subscribe(peerMessage => SyncWithPeer(peerMessage.Peer)));
        }

        private void Update()
        {
            CheckSynchronisation();
            UpdateView();
        }

        private void CheckSynchronisation()
        {
            var synchronisation = _ongoingSynchronisation;
            if (synchronisation == null)
            {
                if (!Synchronised)
                {
                    SyncWithRandomPeer();
                }
                
                return;
            }

            if (synchronisation.LastUpdate + _configuration.SynchronisationTimeout < DateTime.Now)
            {
                _logger.LogWarning($"Synchronisation with {synchronisation.Peer.NodeAddress} failed: Timeout");
                _ongoingSynchronisation = null;
                SyncWithRandomPeer();
            }
        }

        private void SyncWithRandomPeer()
        {
            var peerToSync = _peerService.Peers.Where(p => p.State.PeerFeatures.OptionalGossipQueries)
                .OrderBy(a => Guid.NewGuid())
                .FirstOrDefault();

            if (peerToSync != null)
            {
                SyncWithPeer(peerToSync);
            }
        }
        
        private void UpdateView()
        {
            try
            {
                if (_pendingNodeAnnouncementMessages.Count == 0 &&
                    _pendingChannelAnnouncementMessages.Count == 0 && _pendingChannelUpdateMessages.Count == 0)
                {
                    return;
                }
            
                lock (_syncLock)
                {
                    var newNodes = CreateOrUpdateNodes(_view, _pendingNodeAnnouncementMessages);
                    _pendingNodeAnnouncementMessages.Clear();

                    var (newChannels, allNewNodes) = CreateOrUpdateChannels(_view, _pendingChannelAnnouncementMessages, _pendingChannelUpdateMessages, newNodes);
                    _pendingChannelAnnouncementMessages.Clear();
                    _pendingChannelUpdateMessages.Clear();
                
                    AddToView(allNewNodes, newChannels);
                    Prune(_view, _dbContext);

                    _dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while updating network view", ex);
            }
        }

        private void AddToView(List<NetworkNode> allNewNodes, List<NetworkChannel> allNewChannels)
        {
            _view.AddNodes(allNewNodes);
            _view.AddChannels(allNewChannels);
            _dbContext.Nodes.AddRange(allNewNodes);
            _dbContext.NetworkChannels.AddRange(allNewChannels);
        }

        private void Prune(NetworkView view, NetworkPersistenceContext dbContext)
        {
            var channelsToPrune = GetChannelsToPrune(_view);
            var nodesToPrune = GetNodesToPrune(_view);
            view.PruneChannels(channelsToPrune);
            view.PruneNodes(nodesToPrune);
            
            dbContext.NetworkChannels.RemoveRange(channelsToPrune);
            dbContext.Nodes.RemoveRange(nodesToPrune);
        }

        private (List<NetworkChannel>, List<NetworkNode>) CreateOrUpdateChannels(NetworkView view,
            List<(IPeer, ChannelAnnouncementMessage)> channelMessages,
            List<(IPeer, ChannelUpdateMessage)> channelUpdateMessages, Dictionary<string, NetworkNode> newNodes)
        {
            var channels = view.GetChannels();
            var nodes = view.GetNodes();
            var newChannels = new List<NetworkChannel>();
            var pendingUpdateMessages = channelUpdateMessages.Select(m => m.Item2).ToList();
            foreach (var message in channelMessages.GroupBy(cm => cm.Item2.ShortChannelIdHex).Select(y => y.Last()))
            {
                var channel = channels.GetValueOrDefault(message.Item2.ShortChannelIdHex);
                var channelUpdateMessage = pendingUpdateMessages.LastOrDefault(m => m.ShortChannelIdHex == message.Item2.ShortChannelIdHex);

                if (channel != null && channelUpdateMessage != null)
                {
                    channel.Update(channelUpdateMessage);
                }
                
                if (channel == null && channelUpdateMessage != null)
                {
                    var node1 = GetOrCreateNode(nodes, message.Item2.NodeId1Hex, newNodes);
                    var node2 = GetOrCreateNode(nodes, message.Item2.NodeId2Hex, newNodes);
                    channel = NetworkChannel.Create(message.Item2, channelUpdateMessage, node1, node2);
                    node1.Node1Channels.Add(channel);
                    node2.Node2Channels.Add(channel);
                    newChannels.Add(channel);
                }

                if (channelUpdateMessage != null)
                {
                    pendingUpdateMessages.Remove(channelUpdateMessage);
                }
            }

            foreach (var updateMessage in pendingUpdateMessages)
            {
                var channel = channels.GetValueOrDefault(updateMessage.ShortChannelIdHex);
                channel?.Update(updateMessage);
            }

            return (newChannels, newNodes.Values.ToList());
        }

        private NetworkNode GetOrCreateNode(IDictionary<string,NetworkNode> nodes, String nodeId, IDictionary<string,NetworkNode> newNodes)
        {
            NetworkNode node;
            bool result = nodes.TryGetValue(nodeId, out node);
            if (!result)
            {
                newNodes.TryGetValue(nodeId, out node);
            }
            
            if (node == null)
            {
                node = NetworkNode.Create(nodeId);
                newNodes.Add(node.Id, node);
            }

            return node;
        }

        private Dictionary<string, NetworkNode> CreateOrUpdateNodes(NetworkView view, List<(IPeer, NodeAnnouncementMessage)> nodeAnnouncementMessages)
        {
            var nodes = view.GetNodes();
            var newNodes = new Dictionary<string, NetworkNode>();
            foreach (var message in nodeAnnouncementMessages.GroupBy(cm => cm.Item2.NodeIdHex).Select(y => y.Last()))
            {
                var existingNode = nodes.GetValueOrDefault(message.Item2.NodeIdHex);
                if (existingNode != null)
                {
                    existingNode.Update(message.Item2);
                }
                else
                {
                    newNodes.Add(message.Item2.NodeIdHex, NetworkNode.Create(message.Item2));
                }
            }

            return newNodes;
        }
        
        private List<NetworkNode> GetNodesToPrune(NetworkView view)
        {
            var nodes = view.GetNodes();
            var nodesToPrune = nodes.Values
                .Where(c => c.LastUpdated < (DateTime.Now - _configuration.NodeTimeout) && c.ChannelCount == 0).ToList();
            
            return nodesToPrune;
        }

        private List<NetworkChannel> GetChannelsToPrune(NetworkView view)
        {
            var channels = view.GetChannels();
            var channelsToPrune = channels.Values.Where(c => c.LastUpdateMessageTimestamp < DateTime.Now - _configuration.ChannelTimeout).ToList();

            channelsToPrune.ForEach(c =>
            {
                c.Node1.Node1Channels.Remove(c);
                c.Node2.Node2Channels.Remove(c);
            });

            return channelsToPrune;
        }
        
        
        private void SyncWithPeer(IPeer peer)
        {
            if (_ongoingSynchronisation != null || !peer.State.PeerFeatures.OptionalGossipQueries)
            {
                return;
            }
            
            uint firstBlockNumber = GetFirstBlockNumber();
            int lastBlockNumber = _blockchainClientService.GetBlockCount() + 1;
            _ongoingSynchronisation = new NetworkSyncDetails(peer, lastBlockNumber);
            _logger.LogDebug($"Synchronize network view with peer {peer.NodeAddress}. Sync Blocks from {firstBlockNumber} to {lastBlockNumber}");
            peer.Messaging.Send(new QueryChannelRangeMessage(_networkParameters.ChainHash, firstBlockNumber, (uint)lastBlockNumber));
        }

        private void OnReplyQueryChannelRange(IPeer peer, ReplyQueryChannelRangeMessage message)
        {
            var synchronisation = _ongoingSynchronisation;
            if (synchronisation == null || synchronisation.Peer != peer)
            {
                _logger.LogWarning($"Received {nameof(ReplyQueryChannelRangeMessage)} but there is no ongoing synchronisation.");
                return;
            }

            if (synchronisation.ShortChannelIds.Count == 0 && message.ShortIds.Count == 0 && message.Complete)
            {
                FinishSynchronisation(peer, synchronisation);
                return;
            }
            
            synchronisation.ResetLastUpdate();
            _logger.LogDebug($"Received {nameof(ReplyQueryChannelRangeMessage)} with {message.ShortIds.Count} channel ids.");
            synchronisation.ShortChannelIds.AddRange(message.ShortIds);
            
            if (message.Complete)
            {
                SyncNextChannels(peer, synchronisation);
            }
        }
        
        private void OnReplyShortChannelIdsDoneMessage(IPeer peer, ReplyShortChannelIdsDoneMessage message)
        {
            var synchronisation = _ongoingSynchronisation;
            if (synchronisation == null || synchronisation.Peer != peer)
            {
                _logger.LogWarning($"Received {nameof(ReplyShortChannelIdsDoneMessage)} but there is no ongoing synchronisation.");
                return;
            }

            UpdateSyncPercentage(synchronisation);
            synchronisation.ResetLastUpdate();
            
            if (synchronisation.ShortChannelIdsPosition >= synchronisation.ShortChannelIds.Count)
            {
                FinishSynchronisation(peer, synchronisation);
            }
            else
            {
                SyncNextChannels(peer, synchronisation);
            }
        }

        private void SyncNextChannels(IPeer peer, NetworkSyncDetails synchronisation)
        {
            List<byte[]> channelsToSync = synchronisation.ShortChannelIds
                                        .Skip(synchronisation.ShortChannelIdsPosition)
                                        .Take(ChannelSynchronisationBatchSize).ToList();
            
            _logger.LogDebug($"Query {channelsToSync.Count} channel ids.");
            peer.Messaging.Send(new QueryShortChannelIdsMessage(_networkParameters.ChainHash, channelsToSync, false));
            synchronisation.ShortChannelIdsPosition += ChannelSynchronisationBatchSize;
        }

        private void UpdateSyncPercentage(NetworkSyncDetails synchronisation)
        {
            var percentage = (float) synchronisation.ShortChannelIdsPosition / synchronisation.ShortChannelIds.Count;
            SyncProgressPercentage = percentage > 1 ? 1 : percentage;
            _syncProgressPercentageProvider.OnNext(SyncProgressPercentage);
        }

        private void FinishSynchronisation(IPeer peer, NetworkSyncDetails synchronisation)
        {
            var peerState = GetOrCreatePeerState(peer.PublicKey);
            peerState.UpdateBlockNumber((uint) synchronisation.SyncToBlock);
            _dbContext.SaveChanges();

            _logger.LogInformation($"Synchronisation with {peer.NodeAddress} done. Received channels: {synchronisation.ShortChannelIds.Count}");
            _ongoingSynchronisation = null;
            Synchronised = true;
            SyncProgressPercentage = 1;
        }

        private uint GetFirstBlockNumber()
        {
            if (!_dbContext.PeerStates.Any())
            {
                return 0;
            }

            return _dbContext.PeerStates
                .Select(ps => ps.LastBlockNumber)
                .OrderByDescending(blockNumber => blockNumber)
                .First();
        }
        
        private PeerNetworkViewState GetOrCreatePeerState(string publicKey)
        {
            var state = _view.GetPeerStates().GetValueOrDefault(publicKey);
            if (state == null)
            {
                state = new PeerNetworkViewState { PeerPublicKey = publicKey } ;
                _view.AddPeerState(state);
                _dbContext.PeerStates.Add(state);
                _dbContext.SaveChanges();
            }

            return state;
        }

        private void OnChannelUpdateMessage(IPeer peer, ChannelUpdateMessage message)
        {
            _pendingChannelUpdateMessages.Add((peer, message));
        }

        private void OnNodeAnnouncement(IPeer peer, NodeAnnouncementMessage message)
        {
            _pendingNodeAnnouncementMessages.Add((peer, message));
        }

        private void OnChannelAnnouncement(IPeer peer, ChannelAnnouncementMessage message)
        {
            _pendingChannelAnnouncementMessages.Add((peer, message));
        }

        public void Dispose()
        {
            _subscriptions.ForEach(subscription => subscription.Dispose());
            _subscriptions.Clear();
            _dbContext.Dispose();
            _eventLoopScheduler?.Dispose();
        }
        
        private class NetworkSyncDetails
        {
            public NetworkSyncDetails(IPeer peer, int syncToBlock)
            {
                Peer = peer;
                SyncToBlock = syncToBlock;
            }
            
            public DateTime LastUpdate { get; private set; }  = DateTime.Now;
            public IPeer Peer { get; }
            public int SyncToBlock { get; }
            public List<byte[]> ShortChannelIds { get; } = new List<byte[]>();
            public int ShortChannelIdsPosition { get; set; }

            public void ResetLastUpdate()
            {
                LastUpdate = DateTime.Now;
            }
        }
    }
}