using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLightning.Cryptography;
using NLightning.Network.Configuration;
using NLightning.Network.Models;
using NLightning.OnChain;
using NLightning.Peer;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;

namespace NLightning.Network.GossipMessages
{
    public class ChannelUpdateMessageValidator : IMessageValidator, IDisposable
    {
        private readonly INetworkViewService _networkViewService;
        private readonly ConcurrentDictionary<string, (ECKeyPair, ECKeyPair)> _channelIdNodeIdsMapping = new ConcurrentDictionary<string, (ECKeyPair, ECKeyPair)>();
        private readonly EventLoopScheduler _eventScheduler = new EventLoopScheduler(); 
        private readonly ILogger _logger;
        private IDisposable _periodicScheduler;
        private NetworkParameters _networkParameters;

        public ChannelUpdateMessageValidator(INetworkViewService networkViewService, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            var viewConfiguration = configuration.GetConfiguration<NetworkViewConfiguration>();
            _networkViewService = networkViewService;
            _logger = loggerFactory.CreateLogger(GetType());
            var newSpan = TimeSpan.FromMilliseconds(viewConfiguration.UpdateInterval.TotalMilliseconds * 2);
            _periodicScheduler = _eventScheduler.SchedulePeriodic(newSpan, ClearCache);
        }
        
        public void Validate(Message message, byte[] rawData)
        {
            if (message is ChannelAnnouncementMessage announcementMessage)
            {
                AddToCache(announcementMessage);
            }
            
            if (message is ChannelUpdateMessage updateMessage)
            {
                ValidateUpdateMessage(updateMessage, rawData);
            }
        }

        private void AddToCache(ChannelAnnouncementMessage announcementMessage)
        {
            _channelIdNodeIdsMapping.AddOrReplace(announcementMessage.ShortChannelIdHex,
                (announcementMessage.NodeId1, announcementMessage.NodeId2));
        }

        private void ClearCache()
        {
            _channelIdNodeIdsMapping.Clear();
        }
        
        private void ValidateUpdateMessage(ChannelUpdateMessage message, byte[] rawData)
        {
            var witness = SHA256.ComputeHash(SHA256.ComputeHash(rawData.SubArray(66, rawData.Length - 66)));
            var channel = _networkViewService.View.FindById(message.ShortChannelIdHex);
            bool node1IsOriginator = ChannelFlags.Parse(message.ChannelFlags).Node1IsOriginator;
            ECKeyPair keyPair;
            
            if (channel == null)
            {
                var cacheEntry = default((ECKeyPair, ECKeyPair));
                _channelIdNodeIdsMapping.TryGetValue(message.ShortChannelIdHex, out cacheEntry);
                if (cacheEntry.Item1 == null || cacheEntry.Item2 == null)
                {
                    throw new MessageValidationException(message, $"ChannelUpdateMessage: Can't validate channel update message without channel announcement. Channel ID: {message.ShortChannelIdHex}");
                }

                keyPair = node1IsOriginator ? cacheEntry.Item1 : cacheEntry.Item2;
            }
            else
            {
                keyPair = new ECKeyPair(node1IsOriginator ? channel.Node1Id : channel.Node2Id);
            }
            
            if (!Secp256K1.VerifySignature(witness, message.Signature, keyPair))
            {
                throw new MessageValidationException(message, "ChannelUpdateMessage: Invalid Signature", true);
            }
            
            if (!message.ChainHash.SequenceEqual(_networkParameters.ChainHash))
            {
                throw new MessageValidationException(message, "ChannelUpdateMessage: Invalid chain hash");
            }

            if (message.Timestamp > DateTime.Now.AddDays(1))
            {
                throw new MessageValidationException(message, "ChannelUpdateMessage: Timestamp is unreasonably far in the future");
            }
        }

        public void Initialize(NetworkParameters networkParameters)
        {
            _networkParameters = networkParameters;
        }
        
        public void Dispose()
        {
            ClearCache();
            _periodicScheduler.Dispose();
            _eventScheduler.Dispose();
        }
    }
}