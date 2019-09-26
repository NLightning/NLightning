using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using NLightning.Network.Models;
using NLightning.Utils.Extensions;

namespace NLightning.Network
{
    public class NetworkView : INetworkView
    {
        private readonly object _syncObject = new object();
        private readonly ILogger<NetworkView> _logger;
        private readonly Dictionary<string, PeerNetworkViewState> _viewStates;
        private readonly Dictionary<string, NetworkChannel> _channels;
        private readonly Dictionary<string, NetworkNode> _nodes;

        public NetworkView(ILoggerFactory loggerFactory, List<PeerNetworkViewState> viewStates, List<NetworkNode> nodes, List<NetworkChannel> channels)
        {
            _logger = loggerFactory.CreateLogger<NetworkView>();
            _viewStates = viewStates.ToDictionary(n => n.PeerNetworkAddress, n => n);
            _nodes = nodes.ToDictionary(n => n.Id, n => n);
            _channels = channels.ToDictionary(n => n.Id, n => n);
        }

        public ReadOnlyDictionary<string, PeerNetworkViewState> GetPeerStates()
        {
            lock (_syncObject)
            {
                return new ReadOnlyDictionary<string, PeerNetworkViewState>(_viewStates);
            }
        }
        
        public ReadOnlyDictionary<string, NetworkChannel> GetChannels()
        {
            lock (_syncObject)
            {
                return new ReadOnlyDictionary<string, NetworkChannel>(_channels);
            }
        }

        public ReadOnlyDictionary<String, NetworkNode> GetNodes()
        {
            lock (_syncObject)
            {
                return new ReadOnlyDictionary<string, NetworkNode>(_nodes);
            }
        }

        public void AddNodes(List<NetworkNode> nodesToUpdate)
        {
            if (nodesToUpdate.Count == 0) return;
            
            lock (_syncObject)
            {
                foreach (var networkNode in nodesToUpdate)
                {
                    _nodes.AddOrReplace(networkNode.Id, networkNode);
                }
                
                _logger.LogDebug($"Added {nodesToUpdate.Count} nodes. Total nodes: {_nodes.Count}");
            }
        }

        public NetworkChannel FindById(String shortChannelId)
        {
            lock (_syncObject)
            {
                var channel = default(NetworkChannel);
                _channels.TryGetValue(shortChannelId, out channel);
                return channel;
            }
        }
        
        public void AddChannels(List<NetworkChannel> channels)
        {
            if (channels.Count == 0) return;
            
            lock (_syncObject)
            {
                foreach (var channel in channels)
                {
                    _channels.AddOrReplace(channel.Id, channel);
                }

                _logger.LogDebug($"Added {channels.Count} channels. Total channels: {_channels.Count}");
            }
        }

        public void PruneChannels(List<NetworkChannel> channelsToPrune)
        {
            if (channelsToPrune.Count == 0) return;
            
            lock (_syncObject)
            {
                int count = 0;
                foreach (var channel in channelsToPrune)
                {
                    if (_channels.Remove(channel.Id))
                    {
                        count++;
                    }
                }

                _logger.LogDebug($"Pruned {count} channels. Total channels: {_channels.Count}");
            }
        }
        
        public void PruneNodes(List<NetworkNode> nodesToPrune)
        {
            if (nodesToPrune.Count == 0) return;
            
            lock (_syncObject)
            {
                int count = 0;
                foreach (var networkNode in nodesToPrune)
                {
                    if (_nodes.Remove(networkNode.Id))
                    {
                        count++;
                    }
                }
                
                _logger.LogDebug($"Pruned {count} nodes. Total nodes: {_nodes.Count}");
            }
        }

        public void AddPeerState(PeerNetworkViewState state)
        {
            lock (_syncObject)
            {
                if (!_viewStates.TryGetValue(state.PeerNetworkAddress, out _))
                    _viewStates.Add(state.PeerNetworkAddress, state);
            }
        }
    }
}