using System;
using NLightning.Peer.Channel.Models;

namespace NLightning.Peer.Channel.Logging.Models
{
    public class LocalChannelLogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public LocalChannelLogEntryType EntryType { get; set; }
        
        public int? ChannelId { get; set; }
        public LocalChannel Channel { get; set; }
        public string TemporaryChannelId { get; set; }
        
        public LocalChannelState? OldState { get; set; }
        public LocalChannelState? State { get; set; }
        public LocalChannelError? Error { get; set; }
        public string LogText { get; set; }
        public string ErrorText { get; set; }
        public string ChannelData { get; set; }
        public string DebugData { get; set; }
        public string AdditionalData { get; set; }
    }
}