using System.Linq;
using Microsoft.Extensions.Configuration;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.Establishment.Messages
{
    public class FundingLockedMessageValidator : MessageValidator<FundingLockedMessage>
    {
        private readonly IChannelService _channelService;

        public FundingLockedMessageValidator(IConfiguration configuration, IChannelService channelService)
        {
            _channelService = channelService;
        }

        protected override void ValidateMessage(FundingLockedMessage message, byte[] rawData)
        {
            var pendingChannel = _channelService.Channels.SingleOrDefault(c => c.ChannelId == message.ChannelId.ToHex());
            if (pendingChannel == null)
            {
                throw new MessageValidationException(message, $"Remote peer sent us a {nameof(FundingLockedMessage)} for an unknown channel.");
            }
        }
    }
}