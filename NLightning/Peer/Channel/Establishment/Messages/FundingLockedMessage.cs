using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Transport.Messaging;

namespace NLightning.Peer.Channel.Establishment.Messages
{
    public class FundingLockedMessage : Message, IChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(36, typeof(FundingLockedMessage),
            new List<Property> {
                new Property("Channel ID", PropertyTypes.ChannelId),
                new Property("Next Per Commitment Point", PropertyTypes.PublicKey)
            }.AsReadOnly());
        
        public byte[] ChannelId { get; set; }
        public ECKeyPair NextPerCommitmentPoint { get; set; }

        public FundingLockedMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            NextPerCommitmentPoint = new ECKeyPair(propertyData[1], false);
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>
            {
                ChannelId,
                NextPerCommitmentPoint.PublicKeyCompressed
            };
        }

    }
}