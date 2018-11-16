using System.Linq;
using Microsoft.Extensions.Configuration;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.ChannelCloseMessages
{
    public class ShutdownMessageValidator : MessageValidator<ShutdownMessage>
    {
        private readonly IChannelService _channelService;

        public ShutdownMessageValidator(IConfiguration configuration, IChannelService channelService)
        {
            _channelService = channelService;
        }

        protected override void ValidateMessage(ShutdownMessage message, byte[] rawData)
        {
            var channel = _channelService.Channels.SingleOrDefault(c => c.ChannelId == message.ChannelId.ToHex());
            if (channel == null)
            {
                throw new MessageValidationException(message, $"Remote peer sent us a {nameof(ShutdownMessage)} for an unknown channel.",
                                                     failChannelId: message.ChannelId);
            }
        }
    }
}