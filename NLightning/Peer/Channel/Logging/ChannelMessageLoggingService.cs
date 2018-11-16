using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLightning.Peer.Channel.Configuration;
using NLightning.Peer.Channel.Logging.Models;
using NLightning.Peer.Channel.Models;
using NLightning.Persistence;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.Logging
{
    public class ChannelMessageLoggingService : IChannelMessageLoggingService, IDisposable
    {
        private readonly object _syncObject = new object();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly LocalPersistenceContext _localPersistenceContext;
        private readonly IPeerService _peerService;
        private readonly IChannelService _channelService;
        private readonly bool _persist;

        public ChannelMessageLoggingService(IServiceScopeFactory scopeFactory, IPeerService peerService,
                                            IChannelService channelService, IConfiguration configuration)
        {
            _localPersistenceContext = scopeFactory.CreateScopedService<LocalPersistenceContext>();
            _peerService = peerService;
            _channelService = channelService;
            _persist = configuration.GetConfiguration<ChannelConfiguration>().PersistChannelLogs;
        }

        public void Initialize()
        {
            if (!_persist)
            {
                return;
            }
            
            _subscriptions.Add(_peerService.IncomingMessageProvider
                .Where(peerMessage => peerMessage.Message is IChannelMessage)
                .Subscribe(peerMessage => OnIncomingChannelMessage(peerMessage.Message)));
            
            _subscriptions.Add(_peerService.IncomingMessageProvider
                .Where(peerMessage => peerMessage.Message is ITemporaryChannelMessage)
                .Subscribe(peerMessage => OnIncomingTemporaryChannelMessage(peerMessage.Message)));
            
            _subscriptions.Add(_peerService.OutgoingMessageProvider
                .Where(peerMessage => peerMessage.Message is IChannelMessage)
                .Subscribe(peerMessage => OnOutgoingChannelMessage(peerMessage.Message)));
            
            _subscriptions.Add(_peerService.OutgoingMessageProvider
                .Where(peerMessage => peerMessage.Message is ITemporaryChannelMessage)
                .Subscribe(peerMessage => OnOutgoingTemporaryChannelMessage(peerMessage.Message)));
            
            _subscriptions.Add(_peerService.ValidationExceptionProvider
                .Where(peerMessage => peerMessage.ValidationException.FailChannelId != null)
                .Subscribe(peerMessage => OnValidationException(peerMessage.ValidationException)));
        }

        private void OnValidationException(MessageValidationException validationException)
        {
            var channel = _channelService.Find(validationException.FailChannelId.ToHex());

            if (channel != null)
            {
                LogMessageValidationException(channel, channel.TemporaryChannelId,
                    validationException.MessageToValidate,
                    LocalChannelLogEntryType.MessageValidationException, validationException.Message);
                return;
            }

            var pendingChannel = _channelService.PendingChannels.SingleOrDefault(p =>
                p.TemporaryChannelId == validationException.FailChannelId.ToHex());
            if (pendingChannel != null)
            {
                var persistedChannel = pendingChannel.Channel.Id != 0 ? pendingChannel.Channel : null;
                LogMessageValidationException(persistedChannel, validationException.FailChannelId.ToHex(),
                    validationException.MessageToValidate,
                    LocalChannelLogEntryType.MessageValidationException, validationException.Message);
            }
        }
        
        private void LogMessageValidationException(LocalChannel channel, string temporaryChannelId, Message message,
            LocalChannelLogEntryType type, string errorText)
        {
            var entry = new LocalChannelLogEntry
            {
                ChannelId = channel != null && channel.Id != 0 ? channel.Id : (int?)null,
                Timestamp = DateTime.Now,
                EntryType = type,
                TemporaryChannelId = temporaryChannelId,
                State = channel?.State,
                Error = LocalChannelError.ValidationError,
                ErrorText =  errorText,
                AdditionalData = message.ToString()
            };

            Save(entry);
        }

        private void OnOutgoingTemporaryChannelMessage(Message message)
        {
            byte[] temporaryChannelId = ((ITemporaryChannelMessage) message).TemporaryChannelId;
            var pendingChannel =  _channelService.PendingChannels.SingleOrDefault(p => p.TemporaryChannelId == temporaryChannelId.ToHex());
            if (pendingChannel != null)
            {
                var channel = pendingChannel.Channel != null && pendingChannel.Channel.Id != 0 ? pendingChannel.Channel : null;
                LogChannelMessage(channel, pendingChannel.TemporaryChannelId, message, LocalChannelLogEntryType.OutgoingMessage);   
            }
        }

        private void OnOutgoingChannelMessage(Message message)
        {
            byte[] channelId = ((IChannelMessage) message).ChannelId;
            var channel = _channelService.Find(channelId.ToHex());
            if (channel != null)
            {
                LogChannelMessage(channel, channel.TemporaryChannelId, message, LocalChannelLogEntryType.OutgoingMessage);   
            }
        }

        private void OnIncomingTemporaryChannelMessage(Message message)
        {
            byte[] temporaryChannelId = ((ITemporaryChannelMessage) message).TemporaryChannelId;
            var pendingChannel =  _channelService.PendingChannels.SingleOrDefault(p => p.TemporaryChannelId == temporaryChannelId.ToHex());
            if (pendingChannel != null)
            {
                var channel =  pendingChannel.Channel != null && pendingChannel.Channel.Id != 0 ? pendingChannel.Channel : null;
                LogChannelMessage(channel, pendingChannel.TemporaryChannelId, message, LocalChannelLogEntryType.IncomingMessage);   
            }
        }

        private void OnIncomingChannelMessage(Message message)
        {
            byte[] channelId = ((IChannelMessage) message).ChannelId;
            var channel = _channelService.Find(channelId.ToHex());
            if (channel != null)
            {
                LogChannelMessage(channel, channel.TemporaryChannelId, message, LocalChannelLogEntryType.IncomingMessage);   
            }
            else
            {
                var pendingChannel = _channelService.PendingChannels
                    .Where(c => c.Channel != null)
                    .SingleOrDefault(p => p.Channel.ChannelId == channelId.ToHex());

                if (pendingChannel != null)
                {
                    LogChannelMessage(null, pendingChannel.TemporaryChannelId, message, LocalChannelLogEntryType.IncomingMessage);  
                }
            }
        }

        private void LogChannelMessage(LocalChannel channel, string temporaryChannelId, Message message,
                                        LocalChannelLogEntryType type)
        {
            var entry = new LocalChannelLogEntry
            {
                ChannelId = channel != null && channel.Id != 0 ? channel.Id : (int?)null,
                Timestamp = DateTime.Now,
                EntryType = type,
                TemporaryChannelId = temporaryChannelId,
                State = channel?.State,
                AdditionalData = message.ToString()
            };

            Save(entry);
        }

        private void Save(LocalChannelLogEntry entry)
        {
            lock (_syncObject)
            {
                _localPersistenceContext.Add(entry);
                _localPersistenceContext.SaveChanges();
            }
        }

        public void Dispose()
        {
            _subscriptions.ForEach(s => s.Dispose());
        }
    }
}