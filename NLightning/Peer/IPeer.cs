using NLightning.Transport;
using NLightning.Transport.Messaging;

namespace NLightning.Peer
{
    public interface IPeer
    {
        PeerState State { get; }
        IMessagingClient Messaging { get; }
        NodeAddress NodeAddress { get; }
        string PublicKey { get; }
        bool Reconnect { get; set; }
        void Connect();
        void Disconnect();
    }
}