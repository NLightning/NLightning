using NBitcoin;

namespace NLightning.OnChain.Client
{
    public class Utxo
    {
        public long AmountSatoshi { get; set; }
        public OutPoint OutPoint { get; set; }
        public Script ScriptPubKey { get; set; }
    }
}