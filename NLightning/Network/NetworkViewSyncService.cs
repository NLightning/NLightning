using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using NLightning.Network.Configuration;
using NLightning.Network.GossipMessages;
using NLightning.Network.Models;
using NLightning.Network.QueryMessages;
using NLightning.OnChain;
using NLightning.OnChain.Client;
using NLightning.Peer;
using NLightning.Persistence;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Network
{
    public class NetworkViewSyncService : INetworkViewSyncService, IDisposable
    {
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
        private readonly IBlockchainService _blockchainService;
        private EventLoopScheduler _eventLoopScheduler;
        private NetworkPersistenceContext _dbContext;
        private NetworkView _view;
        
        public NetworkViewSyncService(ILoggerFactory loggerFactory, INetworkViewService networkViewService, IBlockchainService blockchainService,
                                    IPeerService peerService, IConfiguration configuration, IBlockchainClientService blockchainClientService)
        {
            _networkViewService = networkViewService;
            _dbContext = networkViewService.NetworkPersistenceContext;
            _blockchainService = blockchainService;
            _logger = loggerFactory.CreateLogger(GetType());
            _peerService = peerService;
            _blockchainClientService = blockchainClientService;
            _configuration = configuration.GetConfiguration<NetworkViewConfiguration>();
        }

        public void Initialize()
        {
            if (_configuration.SynchronisationMode != SynchronisationMode.Automatic)
            {
                return;
            }

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
            _subscriptions.Add(_eventLoopScheduler.SchedulePeriodic(_configuration.UpdateInterval, UpdateView));
        }
        
        private void SubscribeToEvents()
        {
            _subscriptions.Add(_peerService.IncomingMessageProvider
                .ObserveOn(_eventLoopScheduler)
                .Subscribe(message =>
                {
                    if (message.Item2 is ChannelAnnouncementMessage channelAnnouncementMessage)
                    {
                        OnChannelAnnouncement(message.Item1, channelAnnouncementMessage);
                    }
                    
                    if (message.Item2 is ChannelUpdateMessage channelUpdateMessage)
                    {
                        OnChannelUpdateMessage(message.Item1, channelUpdateMessage);
                    }
                    
                    if (message.Item2 is NodeAnnouncementMessage nodeAnnouncementMessage)
                    {
                        OnNodeAnnouncement(message.Item1, nodeAnnouncementMessage);
                    }
                    
                    if (message.Item2 is ReplyQueryChannelRangeMessage replyQueryChannelRangeMessage)
                    {
                        OnReplyQueryChannelRange(message.Item1, replyQueryChannelRangeMessage);
                    }
                }));

            _subscriptions.Add(_peerService.MessagingStateProvider
                .Delay(TimeSpan.FromSeconds(5))
                .ObserveOn(_eventLoopScheduler)
                .Where(tuple => tuple.Item2 == MessagingClientState.Active)
                .Subscribe(peerMessage => SyncWithPeer(peerMessage.Item1)));
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

        private void OnReplyQueryChannelRange(IPeer peer, ReplyQueryChannelRangeMessage message)
        {
            _logger.LogDebug($"Received ReplyQueryChannelRangeMessage with {message.ShortIds.Count} channel ids.");
            var state = GetOrCreatePeerState(peer.PublicKey);
            var syncToBlock = Math.Min(message.FirstBlockNumber + message.NumberOfBlocks, _blockchainClientService.GetBlockCount());
            
            state.UpdateBlockNumber((uint)syncToBlock);
            _dbContext.SaveChanges();
            
            SyncChannels(peer, message.ShortIds.ToList());
        }

        private void SyncChannels(IPeer peer, List<byte[]> shortChannelIds)
        {
            foreach (var channelIds in shortChannelIds.Batch(1000))
            {
                var list = channelIds.ToList();
                _logger.LogDebug($"Query {list.Count} channel ids.");
                peer.Messaging.Send(new QueryShortChannelIdsMessage(_blockchainService.NetworkParameters.ChainHash, list, false));
            }
        }

        private void SyncWithPeer(IPeer peer)
        {
            uint firstBlockNumber = GetFirstBlockNumber();
            int lastBlockNumber = _blockchainClientService.GetBlockCount() + 1;
            _logger.LogDebug($"Synchronize network view with peer {peer.NodeAddress}. Sync Blocks from {firstBlockNumber} to {lastBlockNumber}");
            peer.Messaging.Send(new QueryChannelRangeMessage(_blockchainService.NetworkParameters.ChainHash, firstBlockNumber, (uint)lastBlockNumber));
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
                .Take(2).Last();
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
       
        public NetworkView View => _view;

        public void Dispose()
        {
            _subscriptions.ForEach(subscription => subscription.Dispose());
            _subscriptions.Clear();
            _dbContext.Dispose();
            _eventLoopScheduler?.Dispose();
        }
    }
}