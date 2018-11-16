using System.Collections.Generic;
using System.Collections.ObjectModel;
using NLightning.Network.GossipMessages;
using NLightning.Network.QueryMessages;
using NLightning.Peer.Channel.ChannelCloseMessages;
using NLightning.Peer.Channel.ChannelEstablishmentMessages;
using NLightning.Peer.Channel.CommitmentMessages;
using NLightning.Peer.Channel.HtlcMessages;
using NLightning.Transport.Messaging.ControlMessages;
using NLightning.Transport.Messaging.SetupMessages;

namespace NLightning.Transport.Messaging
{
    public static class MessageDefinitions
    {
        public static readonly ReadOnlyCollection<MessageDefinition> Definitions = new ReadOnlyCollection<MessageDefinition>(
            new List<MessageDefinition>
            {
                // BOLT #1: Setup Messages
                InitMessage.MessageDefinition,
                ErrorMessage.MessageDefinition,
                
                // BOLT #1: Control Messages
                PingMessage.MessageDefinition,
                PongMessage.MessageDefinition,
                
                // BOLT #2: Channel Establishment Messages
                OpenChannelMessage.MessageDefinition,
                AcceptChannelMessage.MessageDefinition,
                FundingCreatedMessage.MessageDefinition,
                FundingLockedMessage.MessageDefinition,
                FundingSignedMessage.MessageDefinition,
                
                // BOLT #2: Channel Close
                ShutdownMessage.MessageDefinition,
                ClosingSignedMessage.MessageDefinition,
                
                // BOLT #2: Normal Operation 
                UpdateAddHtlcMessage.MessageDefinition,
                UpdateFailHtlcMessage.MessageDefinition,
                UpdateFulfillHtlcMessage.MessageDefinition,
                UpdateFailMalformedHtlcMessage.MessageDefinition,
                RevokeAndAckMessage.MessageDefinition,
                CommitmentSignedMessage.MessageDefinition,
                UpdateFeeMessage.MessageDefinition,
                ChannelReestablishMessage.MessageDefinition,
                
                // BOLT #7: Gossip Messages
                AnnouncementSignaturesMessage.MessageDefinition,
                ChannelAnnouncementMessage.MessageDefinition,
                ChannelUpdateMessage.MessageDefinition,
                GossipTimestampFilterMessage.MessageDefinition,
                NodeAnnouncementMessage.MessageDefinition,
                QueryChannelRangeMessage.MessageDefinition,
                QueryShortChannelIdsMessage.MessageDefinition,
                ReplyQueryChannelRangeMessage.MessageDefinition,
                ReplyShortChannelIdsDoneMessage.MessageDefinition,
            });
    }
}