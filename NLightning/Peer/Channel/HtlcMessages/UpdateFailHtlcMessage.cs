using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.HtlcMessages
{
    public class UpdateFailHtlcMessage : Message, IChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(131, typeof(UpdateFailHtlcMessage),
            new List<Property> {
                new Property("Channel ID", PropertyTypes.ChannelId),
                new Property("Id", PropertyTypes.ULong),
                new Property("Reason", PropertyTypes.VariableArray)
            }.AsReadOnly());
        
        public byte[] ChannelId { get; set; }
        public ulong Id { get; set; }
        public byte[] Reason { get; set; }
        
        public UpdateFailHtlcMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            Id = propertyData[1].ToULongBigEndian();
            Reason = propertyData[2];
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>
            {
                ChannelId,
                Id.GetBytesBigEndian(),
                Reason
            };
        }

    }
}