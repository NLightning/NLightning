using System;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLightning.Network;
using NLightning.Peer.Configuration;
using NLightning.Peer.Discovery;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.AutoConnect
{
    public class PeerAutoConnectService : IPeerAutoConnect, IDisposable
    {
        private readonly IPeerService _peerService;
        private readonly ILogger<PeerService> _logger;
        private readonly PeerConfiguration _configuration;
        private NetworkParameters _networkParameters;
        private EventLoopScheduler _taskScheduler;
        private IDisposable _timer;

        public PeerAutoConnectService(IPeerService peerService, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _peerService = peerService;
            _logger = loggerFactory.CreateLogger<PeerService>();
            _configuration = configuration.GetConfiguration<PeerConfiguration>();
        }

        public void Initialize(NetworkParameters networkParameters)
        {
            if (!_configuration.AutoConnect)
            {
                return;
            }

            _taskScheduler = new EventLoopScheduler();
            
            _networkParameters = networkParameters;
            InitTimer();
        }

        private void InitTimer()
        {
            _timer = _taskScheduler.SchedulePeriodic(_configuration.AutoConnectPeerCheckInterval, CheckAndAddPeers);
        }

        private void CheckAndAddPeers()
        {
            if (_peerService.PeerCount >= _configuration.AutoConnectPeerCountMinimum)
            {
                return;
            }

            try
            {
                var peerCountToAdd = _configuration.AutoConnectPeerCountMinimum - _peerService.PeerCount;
                var dnsBootstrap = new DnsBootstrap(_logger, _networkParameters, _configuration.AutoConnectToIpV6Peers);
                var nodes = dnsBootstrap.FindNodes(peerCountToAdd);
                
                _logger.LogDebug($"Auto connect: DNS bootstrap found {nodes.Count} nodes");
                
                foreach (var nodeAddress in nodes.Except(_peerService.Peers.Select(m => m.NodeAddress)))
                {
                    try
                    {
                        _logger.LogDebug($"Auto connect to {nodeAddress.Address}");
                        _peerService.AddPeer(nodeAddress);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError($"Failed to (auto) connect to {nodeAddress.Address}. Exception: {exception}", exception);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Auto connect failed. {ex}", ex);
            }
        }

        public void Dispose()
        {
            _taskScheduler?.Dispose();
            _timer?.Dispose();
        }
    }
}