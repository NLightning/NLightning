using Microsoft.Extensions.Configuration;
using NLightning.Transport.Messaging.Validation;

namespace NLightning.Peer.Channel.Establishment.Messages
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