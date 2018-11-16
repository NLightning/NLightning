using System.Collections.Generic;
using NLightning.Network;
using NLightning.Transport;

namespace NLightning.Peer.Discovery
{
    public interface IPeerDiscovery
    {
        List<NodeAddress> FindNodes(int count);
    }
}