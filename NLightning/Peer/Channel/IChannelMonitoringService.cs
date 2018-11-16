using NLightning.Network;

namespace NLightning.Peer.Channel
{
    public interface IChannelMonitoringService
    {
        void Initialize(NetworkParameters networkParameters);
    }
}