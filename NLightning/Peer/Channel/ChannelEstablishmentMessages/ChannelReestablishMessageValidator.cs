using Microsoft.Extensions.Configuration;
using NLightning.Transport.Messaging.Validation;

namespace NLightning.Peer.Channel.ChannelEstablishmentMessages
{
    public class ChannelReestablishMessageValidator : MessageValidator<ChannelReestablishMessage>
    {
        public ChannelReestablishMessageValidator(IConfiguration configuration)
        {

        }

        protected override void ValidateMessage(ChannelReestablishMessage message, byte[] rawData)
        {
        }
    }
}