using System;

namespace NLightning.OnChain.Client
{
    public class BlockchainClientException : Exception
    {
        public BlockchainClientException(string message, Exception innerException = null) : base (message, innerException)
        {
            
        }
    }
}