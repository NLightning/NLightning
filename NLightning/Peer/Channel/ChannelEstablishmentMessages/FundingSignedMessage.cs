using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.ChannelEstablishmentMessages
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