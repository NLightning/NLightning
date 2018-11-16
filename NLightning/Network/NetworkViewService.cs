using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLightning.Network.Configuration;
using NLightning.Persistence;
using NLightning.Utils.Extensions;

namespace NLightning.Network
{
    public class NetworkViewService : INetworkViewService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly NetworkViewConfiguration _configuration;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly ILoggerFactory _loggerFactory;
        private readonly NetworkPersistenceContext _dbContext;
        private readonly EventLoopScheduler _taskScheduler = new EventLoopScheduler();
        private NetworkView _view;
        
        public NetworkViewService(ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger(GetType());
            _configuration = configuration.GetConfiguration<NetworkViewConfiguration>();
            _dbContext = scopeFactory.CreateScopedService<NetworkPersistenceContext>();
        }

        public void Initialize()
        {
            LoadViewFromDb();
        }

        public NetworkPersistenceContext NetworkPersistenceContext => _dbContext;
        
        private void LoadViewFromDb()
        {
            var nodes = _dbContext.Nodes.Include(node => node.Node1Channels)
                                        .Include(node => node.Node2Channels).ToList();
            var channels = nodes.SelectMany(node => node.GetAllChannels()).Distinct().ToList();
            var peerStates = _dbContext.PeerStates.ToList();

            _logger.LogInformation($"Loaded {nodes.Count} nodes and {channels.Count} channels from data store.");
            _view = new NetworkView(_loggerFactory, peerStates, nodes, channels);
        }

        public NetworkView View => _view;

        public void Dispose()
        {
            _subscriptions.ForEach(subscription => subscription.Dispose());
            _subscriptions.Clear();
            _dbContext.Dispose();
            _taskScheduler.Dispose();
        }
    }
}