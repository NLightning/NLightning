using System.Linq;
using Microsoft.Extensions.Configuration;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;

namespace NLightning.Peer.Channel.ChannelEstablishmentMessages
{
    public class FundingCreatedMessageValidator : MessageValidator<FundingCreatedMessage>
    {
        private readonly IChannelService _channelService;

        public FundingCreatedMessageValidator(IConfiguration configuration, IChannelService channelService)
        {
            _channelService = channelService;
        }

        protected override void ValidateMessage(FundingCreatedMessage message, byte[] rawData)
        {
            var pendingChannel = _channelService.PendingChannels.SingleOrDefault(c => c.OpenMessage.TemporaryChannelId.SequenceEqual(message.TemporaryChannelId));
            if (pendingChannel == null)
            {
                throw new MessageValidationException(message, $"Remote peer sent us a {nameof(FundingCreatedMessage)} for an unknown channel.");
            }
        }
    }
}