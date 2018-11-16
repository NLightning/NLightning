namespace NLightning.Bitcoind.Configuration
{
    public class BitcoindClientConfiguration
    {
        public string RpcIpAddress { get; set; } = "127.0.0.1";
        public ushort RpcPort { get; set; } = 18332;
        public string RpcUser { get; set; }
        public string RpcPassword { get; set; }
        
    }
}