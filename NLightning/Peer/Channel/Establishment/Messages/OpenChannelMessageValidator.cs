using Microsoft.Extensions.Configuration;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;

namespace NLightning.Peer.Channel.Establishment.Messages
{
    public class OpenChannelMessageValidator : MessageValidator<OpenChannelMessage>
    {
        public OpenChannelMessageValidator(IConfiguration configuration)
        {

        }

        protected override void ValidateMessage(OpenChannelMessage message, byte[] rawData)
        {
            throw new MessageValidationException(message, $"Peer sent us an {nameof(OpenChannelMessage)}. We don't yet support incoming channels.", 
                failChannelId: message.TemporaryChannelId);
        }
    }
}