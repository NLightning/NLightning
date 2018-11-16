using System;
using NLightning.OnChain;
using NLightning.Transport;

namespace NLightning.Peer
{
    public class PeerState
    {
        public PeerState(PeerFeatures peerFeatures)
        {
            PeerFeatures = peerFeatures;
        }
        
        public PeerFeatures PeerFeatures { get; }
        public PeerFeatures RemoteFeatures { get; set; }
        public TransportState TransportState { get; set; }
        public TimeSpan Latency { get; set; } = TimeSpan.Zero;
    }
}