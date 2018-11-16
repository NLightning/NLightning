using System.Linq;
using Microsoft.Extensions.Configuration;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.ChannelCloseMessages
{
    public class ClosingSignedMessageValidator : MessageValidator<ClosingSignedMessage>
    {
        private readonly IChannelService _channelService;

        public ClosingSignedMessageValidator(IConfiguration configuration, IChannelService channelService)
        {
            _channelService = channelService;
        }

        protected override void ValidateMessage(ClosingSignedMessage message, byte[] rawData)
        {
            var channel = _channelService.Channels.SingleOrDefault(c => c.ChannelId == message.ChannelId.ToHex());
            if (channel == null)
            {
                throw new MessageValidationException(message, $"Remote peer sent us a {nameof(ClosingSignedMessage)} for an unknown channel.",
                    failChannelId: message.ChannelId);
            }
        }
    }
}