namespace NLightning.Wallet
{    
    public enum KeyFamily 
    {
        MultiSig = 0,
        RevocationBase = 1,
        HtlcBase = 2,
        PaymentBase = 3,
        DelayBase = 4,
        RevocationRoot = 5,
        NodeKey = 6
    }
}