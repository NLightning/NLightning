namespace NLightning.Wallet.Commitment
{
    public static class TransactionFee
    {
        public const ulong CommitWeight = 724;
        public const ulong HtlcTimeoutWeight = 663;
        public const ulong HtlcSuccessWeight = 703;
        public const ulong ClaimP2WpkhOutputWeight = 439;
        public const ulong ClaimHtlcDelayedWeight = 484;
        public const ulong ClaimHtlcSuccessWeight = 572;
        public const ulong ClaimHtlcTimeoutWeight = 546;
        public const ulong MainPenaltyWeight = 485;
        public const ulong HtlcPenaltyWeight = 579;

        public static ulong CalculateRate(ulong fee, ulong weight)
        {
            return (fee * 1000L) / weight;
        }

        public static ulong CalculateFee(ulong feeratePerKw, ulong weight)
        {
            return (feeratePerKw * weight) / 1000;
        }
    }
}