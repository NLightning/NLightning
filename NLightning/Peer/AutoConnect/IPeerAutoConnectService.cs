using NLightning.Network;

namespace NLightning.Peer.AutoConnect
{
    public interface IPeerAutoConnect
    {
        void Initialize(NetworkParameters networkParameters);
    }
}