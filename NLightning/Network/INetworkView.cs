using System.Collections.ObjectModel;
using NLightning.Network.Models;

namespace NLightning.Network
{
    public interface INetworkView
    {
        ReadOnlyDictionary<string, PeerNetworkViewState> GetPeerStates();
        ReadOnlyDictionary<string, NetworkChannel> GetChannels();
        ReadOnlyDictionary<string, NetworkNode> GetNodes();
    }
}