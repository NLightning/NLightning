using NLightning.Peer.Channel.Logging.Models;
using NLightning.Peer.Channel.Models;

namespace NLightning.Peer.Channel.Logging
{
    public interface IChannelLoggingService
    {
        void LogStateUpdate(LocalChannel channel, LocalChannelState oldState, string logText = null, string additionalData = null);
        void LogInfo(LocalChannel channel, string logText, string additionalData = null);
        void LogWarning(LocalChannel channel, string logText, string additionalData = null);
        void LogError(LocalChannel channel, LocalChannelError error, string errorText, string additionalData = null);
        void LogPendingChannelInfo(string temporaryChannelId, LocalChannelState state, string logText = null);
        void LogPendingChannelError(string temporaryChannelId, LocalChannelState state, LocalChannelError error, string errorText);
    }
}