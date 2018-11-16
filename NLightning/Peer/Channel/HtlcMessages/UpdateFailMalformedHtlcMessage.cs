using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.HtlcMessages
{
    public class UpdateFailMalformedHtlcMessage : Message, IChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(135, typeof(UpdateFailMalformedHtlcMessage),
            new List<Property> {
                new Property("Channel ID", PropertyTypes.ChannelId),
                new Property("Id", PropertyTypes.ULong),
                new Property("Sha256 Of Onion", PropertyTypes.Hash32),
                new Property("Failure Code", PropertyTypes.UShort)
            }.AsReadOnly());
        
        public byte[] ChannelId { get; set; }
        public ulong Id { get; set; }
        public byte[] Sha256OfOnion { get; set; }
        public ushort FailureCode { get; set; }
        
        public UpdateFailMalformedHtlcMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            Id = propertyData[1].ToULongBigEndian();
            Sha256OfOnion = propertyData[2];
            FailureCode = propertyData[3].ToUShortBigEndian();
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>
            {
                ChannelId,
                Id.GetBytesBigEndian(),
                Sha256OfOnion,
                FailureCode.GetBytesBigEndian()
            };
        }

    }
}