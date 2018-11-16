using NLightning.Network;

namespace NLightning.OnChain
{
    public interface IBlockchainService
    {
        NetworkParameters NetworkParameters { get; }

        void Initialize(NetworkParameters networkParameters);
    }
}