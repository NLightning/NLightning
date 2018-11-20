using System.Linq;
using Microsoft.Extensions.Configuration;
using NLightning.Peer.Channel.Configuration;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.Establishment.Messages
{
    public class AcceptChannelMessageValidator : MessageValidator<AcceptChannelMessage>
    {
        private readonly IChannelService _channelService;
        private ChannelConfiguration _configuration;

        public AcceptChannelMessageValidator(IConfiguration configuration, IChannelService channelService)
        {
            _channelService = channelService;
            _configuration = configuration.GetConfiguration<ChannelConfiguration>();
        }

        protected override void ValidateMessage(AcceptChannelMessage message, byte[] rawData)
        {
            var channel = _channelService.Channels.SingleOrDefault(c => c.ChannelId == message.TemporaryChannelId.ToHex());
            if (channel != null)
            {
                throw new MessageValidationException(message, $"Remote peer sent us an {nameof(AcceptChannelMessage)} for an already established channel.");
            }

            var pendingChannel = _channelService.PendingChannels.SingleOrDefault(c => c.OpenMessage.TemporaryChannelId.SequenceEqual(message.TemporaryChannelId));
            if (pendingChannel == null)
            {
                throw new MessageValidationException(message, $"Remote peer sent us an {nameof(AcceptChannelMessage)} for an unknown channel.");
            }
            
            if (message.MinimumDepth > _configuration.MinimumDepthMax)
            {
                string errMessage = $"Unreasonably large minimum depth. Max: {_configuration.MinimumDepthMax}";
                throw new MessageValidationException(message, errMessage, false, message.TemporaryChannelId);
            }
            
            if (message.ChannelReserveSatoshis < pendingChannel.OpenMessage.DustLimitSatoshis)
            {
                string errMessage = $"ChannelReserveSatoshis is less than DustLimitSatoshis. " +
                                    $"OpenMessage.DustLimitSatoshis: {pendingChannel.OpenMessage.DustLimitSatoshis}." +
                                    $"AcceptMessage.ChannelReserveSatoshis: {message.ChannelReserveSatoshis}.";
                throw new MessageValidationException(message, errMessage, false, message.TemporaryChannelId);
            }
            
            if (pendingChannel.OpenMessage.ChannelReserveSatoshis < message.DustLimitSatoshis)
            {
                string errMessage = $"ChannelReserveSatoshis is less than DustLimitSatoshis. " +
                                    $"AcceptMessage.DustLimitSatoshis: {message.DustLimitSatoshis}." +
                                    $"OpenMessage.ChannelReserveSatoshis: {pendingChannel.OpenMessage.ChannelReserveSatoshis}.";
                throw new MessageValidationException(message, errMessage, false, message.TemporaryChannelId);
            }
        }
    }
}