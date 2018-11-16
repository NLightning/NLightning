using System;
using System.Collections.Generic;
using NLightning.Utils.Extensions;

namespace NLightning.Network
{
    public class NetworkParameters
    {
        public static readonly NetworkParameters BitcoinMainnet = new NetworkParameters()
        {
            Name = "btc-mainnet", 
            ChainHash = "6fe28c0ab6f1b372c1a6a246ae63f74f931e8365e15a089c68d6190000000000".HexToByteArray(), 
            CoinType = 0,
            Network = NBitcoin.Network.Main,
            DnsSeeds = new List<string>()
            {
                "nodes.lightning.directory"
            },
            DnsNetworkRealm = 0
        };
        
        public static readonly NetworkParameters BitcoinTestnet = new NetworkParameters()
        {
            Name = "btc-testnet", 
            ChainHash = "43497fd7f826957108f4a30fd9cec3aeba79972084e90ead01ea330900000000".HexToByteArray(),
            CoinType = 1,
            Network = NBitcoin.Network.TestNet,
            DnsSeeds = new List<string>()
            {
                "test.nodes.lightning.directory"
            },
            DnsNetworkRealm = 0
        };
        
        private NetworkParameters()
        {
        }

        public String Name { get; private set; }
        public byte[] ChainHash { get; private set; }
        public int CoinType { get; private set; }
        public NBitcoin.Network Network { get; private set; }
        public List<string> DnsSeeds { get; private set; }
        public int DnsNetworkRealm { get; set; }
    }
}