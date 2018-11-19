using System;

namespace NLightning.Peer.Configuration
{
    public class PeerConfiguration
    {
        public bool AutoConnect { get; set; } = true;
        public bool AutoConnectToIpV6Peers { get; set; } = false;
        public ushort AutoConnectPeerCountMinimum { get; set; } = 3;
        public TimeSpan AutoConnectPeerCheckInterval { get; set; } = TimeSpan.FromSeconds(15);
        public TimeSpan ReconnectIntervalMinimum { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan ReconnectIntervalMaximum { get; set; } = TimeSpan.FromMinutes(10);
    }
}