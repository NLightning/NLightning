namespace NLightning.OnChain.Monitoring.Models
{
    public class SpendingTransactionLookup
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public ushort OutputIndex { get; set; }
        public uint LastBlockHeight { get; set; }
    }
}