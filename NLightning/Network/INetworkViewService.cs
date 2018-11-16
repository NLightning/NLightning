using NLightning.Persistence;

namespace NLightning.Network
{
    public interface INetworkViewService
    {
        NetworkView View { get; }
        void Initialize();
        NetworkPersistenceContext NetworkPersistenceContext { get; }
    }
}