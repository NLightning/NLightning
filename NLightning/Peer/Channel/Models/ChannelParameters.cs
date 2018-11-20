using NLightning.Peer.Channel.Establishment.Messages;

namespace NLightning.Peer.Channel.Models
{
    public class ChannelParameters
    {
        public int Id { get; set; }
        public ulong DustLimitSatoshis { get; set; }
        public ulong MaxHtlcValueInFlightMSat { get; set; }
        public ulong ChannelReserveSatoshis { get; set; }
        public ushort ToSelfDelay { get; set; }
        public ulong HtlcMinimumMSat { get; set; }
        public ushort MaxAcceptedHtlcs { get; set; }
        public byte[] ShutdownScriptPubKey { get; set; }
        
        public static ChannelParameters CreateFromOpenMessage(OpenChannelMessage openMessage)
        {
            ChannelParameters channelParameters = new ChannelParameters();

            channelParameters.DustLimitSatoshis = openMessage.DustLimitSatoshis;
            channelParameters.MaxHtlcValueInFlightMSat = openMessage.MaxHtlcValueInFlightMSat;
            channelParameters.ChannelReserveSatoshis = openMessage.ChannelReserveSatoshis;
            channelParameters.ToSelfDelay = openMessage.ToSelfDelay;
            channelParameters.HtlcMinimumMSat = openMessage.HtlcMinimumMSat;
            channelParameters.MaxAcceptedHtlcs = openMessage.MaxAcceptedHtlcs;
            channelParameters.ShutdownScriptPubKey = openMessage.ShutdownScriptPubKey;
            
            return channelParameters;
        }

        public static ChannelParameters CreateFromAcceptMessage(AcceptChannelMessage acceptMessage)
        {
            ChannelParameters channelParameters = new ChannelParameters();

            channelParameters.DustLimitSatoshis = acceptMessage.DustLimitSatoshis;
            channelParameters.MaxHtlcValueInFlightMSat = acceptMessage.MaxHtlcValueInFlightMSat;
            channelParameters.ChannelReserveSatoshis = acceptMessage.ChannelReserveSatoshis;
            channelParameters.ToSelfDelay = acceptMessage.ToSelfDelay;
            channelParameters.HtlcMinimumMSat = acceptMessage.HtlcMinimumMSat;
            channelParameters.MaxAcceptedHtlcs = acceptMessage.MaxAcceptedHtlcs;
            channelParameters.ShutdownScriptPubKey = acceptMessage.ShutdownScriptPubKey;
            
            return channelParameters;
        }
    }
}