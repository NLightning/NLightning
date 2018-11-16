using Microsoft.Extensions.Logging;
using NLightning.Network;
using NLightning.OnChain.Client;

namespace NLightning.OnChain
{
    public class BlockchainService : IBlockchainService
    {
        public BlockchainService(ILoggerFactory loggerFactory, IBlockchainClientService clientService)
        {
            
        }
        
        public NetworkParameters NetworkParameters { get; private set; }

        public void Initialize(NetworkParameters networkParameters)
        {
            NetworkParameters = networkParameters;
        }
    }
}