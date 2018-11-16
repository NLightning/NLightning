namespace NLightning.Peer.Channel.Configuration
{
    public class ChannelConfiguration
    {
        public ushort ToSelfDelay { get; set; } = 144;
        public ulong HtlcMinMSat { get; set; } = 1000;
        public uint FeePerKwMinimum { get; set; } = 253;
        public ulong DustLimit { get; set; } = 546;
        public uint AcceptHtlcMax { get; set; } = 483;
        public ulong FundingSatoshiMax { get; set; } = 16777216;
        public ulong HtlcInFlightMSat { get; set; } = 5000000000;
        public double ReserveToFundingRatio { get; set; } = 0.1;
        public bool AnnounceChannels { get; set; } = false;
        public double ClosingFeeDeviationMaximumPercentage { get; set; } = 1.5;
        public ushort MinimumDepthMax { get; set; } = 133;
        
        public uint CltvExpiryMin = 9;
        public uint CltvExpiryMax = 7 * 144;

        public bool PersistChannelLogs { get; set; } = true;
    }
}