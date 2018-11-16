using System;

namespace NLightning.OnChain.Configuration
{
    public class BlockchainConfiguration
    {
        public TimeSpan BlockchainMonitorInterval { get; set; } = TimeSpan.FromMinutes(1);
    }
}