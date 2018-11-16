using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.HtlcMessages
{
    public class UpdateFulfillHtlcMessage : Message, IChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(130, typeof(UpdateFulfillHtlcMessage),
            new List<Property> {
                new Property("Channel ID", PropertyTypes.ChannelId),
                new Property("Id", PropertyTypes.ULong),
                new Property("Payment Preimage", PropertyTypes.Hash32)
            }.AsReadOnly());
        
        public byte[] ChannelId { get; set; }
        public ulong Id { get; set; }
        public byte[] PaymentPreimage { get; set; }
        
        public UpdateFulfillHtlcMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            Id = propertyData[1].ToULongBigEndian();
            PaymentPreimage = propertyData[2];
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>
            {
                ChannelId,
                Id.GetBytesBigEndian(),
                PaymentPreimage
            };
        }

    }
}