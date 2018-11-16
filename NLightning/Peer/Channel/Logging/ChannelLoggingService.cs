using System;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLightning.Peer.Channel.Configuration;
using NLightning.Peer.Channel.Logging.Models;
using NLightning.Peer.Channel.Models;
using NLightning.Persistence;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.Logging
{
    public class ChannelLoggingService : IChannelLoggingService
    {
        private readonly LocalPersistenceContext _localPersistenceContext;
        private readonly object _syncObject = new object();
        private readonly bool _persist;
        private readonly ILogger _logger;

        public ChannelLoggingService(LocalPersistenceContext localPersistenceContext, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _localPersistenceContext = localPersistenceContext;
            _logger = loggerFactory.CreateLogger(GetType());
            _persist = configuration.GetConfiguration<ChannelConfiguration>().PersistChannelLogs;
        }

        public void LogStateUpdate(LocalChannel channel, LocalChannelState oldState, string logText = "", string additionalData = "")
        {
            var entry = new LocalChannelLogEntry
            {
                Timestamp = DateTime.Now,
                EntryType = LocalChannelLogEntryType.StateUpdate,
                ChannelId = channel != null && channel.Id != 0 ? channel.Id : (int?)null,
                TemporaryChannelId = channel?.TemporaryChannelId,
                ChannelData = channel?.ToString(),
                State = channel?.State,
                OldState = oldState,
                LogText = logText,
                DebugData = GetDebugData(),
                AdditionalData = additionalData
            };

            string channelData = channel != null ? channel.ToString() : "";
            string newState = channel != null ? channel.State.ToString() : "";
            
            _logger.LogDebug($"Channel state update. New state: {newState}. Old state: {oldState}. {logText}. " +
                             $"AdditionalData: {additionalData}. Channel: {channelData}");
            Save(entry);
        }

        private string GetDebugData()
        {   
            var stackTrace = new StackTrace(2, false);
            return stackTrace.ToString();
        }

        public void LogInfo(LocalChannel channel, string logText, string additionalData)
        {
            Log(channel, logText, LocalChannelLogEntryType.Info, additionalData);
        }
        
        public void LogWarning(LocalChannel channel, string logText, string additionalData)
        {
            Log(channel, logText, LocalChannelLogEntryType.Warning, additionalData);
        }
        
        public void Log(LocalChannel channel, string logText, LocalChannelLogEntryType type, string additionalData = "")
        {
            var entry = new LocalChannelLogEntry
            {
                Timestamp = DateTime.Now,
                EntryType = type,
                ChannelId = channel != null && channel.Id != 0 ? channel.Id : (int?)null,
                TemporaryChannelId = channel?.TemporaryChannelId,
                ChannelData = channel?.ToString(),
                State = channel?.State,
                LogText = logText,
                DebugData = GetDebugData(),
                AdditionalData = additionalData
            };

            string channelData = channel != null ? channel.ToString() : "";
            _logger.LogDebug($"{type}: {logText}. AdditionalData: {additionalData}. Channel: {channelData}");
            Save(entry);
        }

        public void LogError(LocalChannel channel, LocalChannelError error, string errorText, string additionalData = "")
        {
            var entry = new LocalChannelLogEntry
            {
                Timestamp = DateTime.Now,
                EntryType = LocalChannelLogEntryType.Error,
                ChannelId = channel != null && channel.Id != 0 ? channel.Id : (int?)null,
                TemporaryChannelId = channel?.TemporaryChannelId,
                ChannelData = channel?.ToString(),
                State = channel?.State,
                Error = error,
                ErrorText = errorText,
                DebugData = GetDebugData(),
                AdditionalData = additionalData
            };

            string channelData = channel != null ? channel.ToString() : "";
            _logger.LogError($"{error}: {errorText}. AdditionalData: {additionalData}. Channel: {channelData}");
            Save(entry);
        }

        public void LogPendingChannelInfo(string temporaryChannelId, LocalChannelState state, string logText = "")
        {
            var entry = new LocalChannelLogEntry
            {
                Timestamp = DateTime.Now,
                EntryType = LocalChannelLogEntryType.Info,
                TemporaryChannelId = temporaryChannelId,
                State = state,
                LogText = logText,
                DebugData = GetDebugData()
            };

            _logger.LogInformation($"{logText}. State: {state}. TemporaryChannelID: {temporaryChannelId}");
            Save(entry);
        }

        public void LogPendingChannelError(string temporaryChannelId, LocalChannelState state, LocalChannelError error,
            string errorText)
        {
            var entry = new LocalChannelLogEntry
            {
                Timestamp = DateTime.Now,
                EntryType = LocalChannelLogEntryType.Error,
                TemporaryChannelId = temporaryChannelId,
                State = state,
                Error = error,
                ErrorText = errorText,
                DebugData = GetDebugData()
            };
            
            _logger.LogError($"{error}: {errorText}. TemporaryChannelID: {temporaryChannelId}");
            Save(entry);
        }
        
        private void Save(LocalChannelLogEntry entry)
        {
            if (!_persist)
            {
                return;
            }

            lock (_syncObject)
            {
                _localPersistenceContext.Add(entry);
                _localPersistenceContext.SaveChanges();
            }
        }
    }
}