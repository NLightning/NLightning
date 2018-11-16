using System.Linq;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.HtlcMessages
{
    public class UpdateAddHtlcMessageValidator : MessageValidator<UpdateAddHtlcMessage>
    {
        private readonly IChannelService _channelService;

        public UpdateAddHtlcMessageValidator(IChannelService channelService)
        {
            _channelService = channelService;
        }
        
        protected override void ValidateMessage(UpdateAddHtlcMessage message, byte[] rawData)
        {
            Models.LocalChannel channel = _channelService.Channels.SingleOrDefault(c => c.ChannelId == message.ChannelId.ToHex());
            byte[] channelId = message.ChannelId;
            if (channel == null)
            {
                throw new MessageValidationException(message, "Unknown Channel");
            }
            
            if (message.AmountMSat == 0 || message.AmountMSat < channel.LocalChannelParameters.HtlcMinimumMSat)
            {
                throw new MessageValidationException(message, "Amount too small.", false, channelId);
            }
            
            if (channel.Htlcs.Count + 1 > channel.LocalChannelParameters.MaxAcceptedHtlcs)
            {
                throw new MessageValidationException(message, "HTLC maximum reached", false, channelId);
            }

            var totalHtlcAmount = (ulong)channel.Htlcs.Select(h => (long)h.AmountMsat).Sum();
            if (totalHtlcAmount + message.AmountMSat > channel.LocalChannelParameters.MaxHtlcValueInFlightMSat)
            {
                throw new MessageValidationException(message, "HTLC maximum value reached", false, channelId);
            }

            if (message.CltvExpiry > 500000000)
            {
                throw new MessageValidationException(message, "Unreasonable high CLTV.", false, channelId);
            }
        }
    }
}