using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.HtlcMessages
{
    public class UpdateAddHtlcMessage : Message, IChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(128, typeof(UpdateAddHtlcMessage),
            new List<Property> {
                new Property("Channel ID", PropertyTypes.ChannelId),
                new Property("Id", PropertyTypes.ULong),
                new Property("Amount MSat", PropertyTypes.ULong),
                new Property("Payment Hash", PropertyTypes.Hash32),
                new Property("Cltv Expiry", PropertyTypes.UInt),
                new Property("Onion Routing Packet", PropertyTypes.RoutingPacket1366)
            }.AsReadOnly());
        
        public byte[] ChannelId { get; set; }
        public ulong Id { get; set; }
        public ulong AmountMSat { get; set; }
        public uint CltvExpiry { get; set; }
        public byte[] PaymentHash { get; set; }
        public byte[] OnionRoutingPacket { get; set; }
        
        public UpdateAddHtlcMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            Id = propertyData[1].ToULongBigEndian();
            AmountMSat = propertyData[2].ToULongBigEndian();
            CltvExpiry = propertyData[3].ToUIntBigEndian();
            PaymentHash = propertyData[4];
            OnionRoutingPacket = propertyData[5];
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>
            {
                ChannelId,
                Id.GetBytesBigEndian(),
                AmountMSat.GetBytesBigEndian(),
                CltvExpiry.GetBytesBigEndian(),
                PaymentHash,
                OnionRoutingPacket
            };
        }

    }
}