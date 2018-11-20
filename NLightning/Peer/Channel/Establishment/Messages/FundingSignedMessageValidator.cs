using System.Linq;
using Microsoft.Extensions.Configuration;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.Establishment.Messages
{
    public class FundingSignedMessageValidator : MessageValidator<FundingSignedMessage>
    {
        private readonly IChannelService _channelService;

        public FundingSignedMessageValidator(IConfiguration configuration, IChannelService channelService)
        {
            _channelService = channelService;
        }

        protected override void ValidateMessage(FundingSignedMessage message, byte[] rawData)
        {
            var pendingChannel = _channelService.PendingChannels.SingleOrDefault(c => c.Channel.ChannelId == message.ChannelId.ToHex());
            if (pendingChannel == null)
            {
                throw new MessageValidationException(message, $"Remote peer sent us a {nameof(FundingSignedMessage)} for an unknown channel.");
            }
        }
    }
}