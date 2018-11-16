using NBitcoin;

namespace NLightning.OnChain.Client
{
    public class TransactionInfo
    {
        public uint Confirmations { get; set; }
        public Transaction Transaction { get; set; }
    }
}