using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using NLightning.Cryptography;
using NLightning.OnChain;
using NLightning.Peer.Channel.Models;
using NLightning.Peer.Configuration;
using NLightning.Persistence;
using NLightning.Transport;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils;
using NLightning.Utils.Extensions;

namespace NLightning.Peer
{
    public class PeerService : IPeerService, IDisposable
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly LocalPersistenceContext _dbContext;
        private readonly IEnumerable<IMessageValidator> _messageValidators;
        private readonly object _syncObject = new object();
        private readonly EventLoopScheduler _taskScheduler = new EventLoopScheduler();
        private readonly Subject<(IPeer, Message)> _incomingMessageProvider = new Subject<(IPeer, Message)>();
        private readonly Subject<(IPeer, Message)> _outgoingMessageProvider = new Subject<(IPeer, Message)>();
        private readonly Subject<(IPeer, MessageValidationException)> _validationExceptionProvider = new Subject<(IPeer, MessageValidationException)>();
        private readonly Subject<(IPeer, MessagingClientState)> _messagingStateProvider = new Subject<(IPeer, MessagingClientState)>();
        private readonly List<IPeer> _peers = new List<IPeer>();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly Dictionary<NodeAddress, PeerReconnect> _peerReconnects = new Dictionary<NodeAddress, PeerReconnect>(); 
        private ECKeyPair _localKey;
        private PeerConfiguration _configuration;
        private ILogger<PeerService> _logger;

        public PeerService(ILoggerFactory loggerFactory, IEnumerable<IMessageValidator> messageValidators,
                            IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<PeerService>();
            _loggerFactory = loggerFactory;
            _messageValidators = messageValidators;
            _dbContext = scopeFactory.CreateScopedService<LocalPersistenceContext>();
            _configuration = configuration.GetConfiguration<PeerConfiguration>();
        }

        public int PeerCount => _peers.Count;
        public IReadOnlyCollection<IPeer> Peers => _peers.AsReadOnly();
        public IObservable<(IPeer, Message)> IncomingMessageProvider => _incomingMessageProvider;
        public IObservable<(IPeer, Message)> OutgoingMessageProvider => _outgoingMessageProvider;
        public IObservable<(IPeer, MessageValidationException)> ValidationExceptionProvider => _validationExceptionProvider;
        public IObservable<(IPeer, MessagingClientState)> MessagingStateProvider => _messagingStateProvider;
        
        public IPeer AddPeer(NodeAddress address, bool persist = false, bool reconnect = false)
        {
            lock (_syncObject)
            {                
                if (_peers.Any(a => a.NodeAddress.Equals(address)))
                {
                    throw new PeerException("Already added this node or a node with the same public key.");
                }
            
                IPeer peer = new LightningPeer(_loggerFactory, _messageValidators, _localKey, address);
                
                SubscribeToEvents(peer);

                peer.Reconnect = reconnect;
                peer.Connect();

                if (peer.Messaging.State != MessagingClientState.Stopped)
                {
                    _peerReconnects.Remove(address);
                    _peers.Add(peer);
                
                    if (persist)
                    {
                        PersistPeer(peer);
                    }
                }

                return peer;
            }
        }

        private void SubscribeToEvents(IPeer peer)
        {
            List<IDisposable> subscriptions = new List<IDisposable>();

            subscriptions.Add(peer.Messaging.IncomingMessageProvider
                .ObserveOn(_taskScheduler)
                .Subscribe(message => _incomingMessageProvider.OnNext((peer, message))));

            subscriptions.Add(peer.Messaging.OutgoingMessageProvider
                .ObserveOn(_taskScheduler)
                .Subscribe(message => _outgoingMessageProvider.OnNext((peer, message))));

            subscriptions.Add(peer.Messaging.ValidationExceptionProvider
                .ObserveOn(_taskScheduler)
                .Subscribe(message => _validationExceptionProvider.OnNext((peer, message))));

            subscriptions.Add(peer.Messaging.StateProvider
                .ObserveOn(_taskScheduler)
                .Subscribe(clientState =>
                {
                    MessagingStateChanged(peer, clientState, subscriptions);
                    _messagingStateProvider.OnNext((peer, clientState));
                }));
            
            _subscriptions.AddRange(subscriptions);
        }

        private void PersistPeer(IPeer peer)
        {
            lock (_syncObject)
            {
                var persistPeer = _dbContext.Peers.SingleOrDefault(p => p.Address == peer.NodeAddress.Address);
                if (persistPeer == null)
                {
                    persistPeer = new PersistentPeer
                    {
                        AutoConnect = true, 
                        Address = peer.NodeAddress.Address
                    };
                    
                    _dbContext.Peers.Add(persistPeer);
                }

                _dbContext.SaveChanges();
            }
        }

        public void Initialize(ECKeyPair localKey)
        {
            _localKey = localKey;
            AddPersistedPeers();
            InitReconnectTimer();
        }

        private void InitReconnectTimer()
        {
            _taskScheduler.SchedulePeriodic(_configuration.ReconnectIntervalMinimum, Reconnect);
        }

        private void Reconnect()
        {
            foreach (var reconnect in _peerReconnects.ToList())
            {
                try
                {
                    if (_peers.Any(p => Equals(p.NodeAddress, reconnect.Key)))
                    {
                        // Already connected. Do not reconnect.
                        _peerReconnects.Remove(reconnect.Key);
                    }
                    else
                    {
                        var interval = ExponentialBackOff.Calculate(reconnect.Value.FailedReconnects,
                            _configuration.ReconnectIntervalMinimum, _configuration.ReconnectIntervalMaximum);
                        var nextAttempt = reconnect.Value.LastReconnect + interval;
                        if (nextAttempt < DateTime.Now)
                        {
                            AddPeer(reconnect.Key, reconnect: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    reconnect.Value.AddFailedReconnect();
                    _logger.LogError($"Failed to connect to peer {reconnect.Key}. " +
                                     $"Reconnect attempt nb. {reconnect.Value.FailedReconnects}. " +
                                     $"Exception: {ex}");
                }
            }
        }

        private void AddPersistedPeers()
        {
            var peers = _dbContext.Peers.Where(p => p.AutoConnect);
            foreach (var persistentPeer in peers)
            {
                var nodeAddress = NodeAddress.Parse(persistentPeer.Address);
                _peerReconnects.TryAdd(nodeAddress, new PeerReconnect(nodeAddress));
            }
        }

        private void MessagingStateChanged(IPeer peer, MessagingClientState clientState, 
            List<IDisposable> subscriptions)
        {
            lock (_syncObject)
            {
                if (clientState == MessagingClientState.Stopped)
                {
                    _peers.Remove(peer);
                    subscriptions.ForEach(sub =>
                    {
                        _subscriptions.Remove(sub);
                        sub.Dispose();
                    });
                
                    if (peer.Reconnect)
                    {
                        _peerReconnects.TryAdd(peer.NodeAddress, new PeerReconnect(peer.NodeAddress));
                    }
                }
            }
        }

        public void Dispose()
        {
            _incomingMessageProvider.Dispose();
            _outgoingMessageProvider.Dispose();
            _validationExceptionProvider.Dispose();
            _messagingStateProvider.Dispose();
            _taskScheduler.Dispose();
            _dbContext.Dispose();
            _subscriptions.ForEach(sub => sub.Dispose());
        }
    }
}