using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.Peer.Channel.Establishment.Messages
{
    public class FundingSignedMessage : Message, IChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(35, typeof(FundingSignedMessage),
            new List<Property> {
                new Property("Channel ID", PropertyTypes.ChannelId),
                new Property("Signature", PropertyTypes.Signature)
            }.AsReadOnly());
        
        public byte[] ChannelId { get; set; }
        public byte[] Signature { get; set; }

        public FundingSignedMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            Signature = propertyData[1];
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>
            {
                ChannelId,
                Signature
            };
        }

    }
}