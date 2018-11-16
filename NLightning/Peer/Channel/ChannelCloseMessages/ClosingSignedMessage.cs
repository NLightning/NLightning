using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.ChannelCloseMessages
{
    public class ClosingSignedMessage : Message, IChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(39, typeof(ClosingSignedMessage),
            new List<Property> {
                new Property("Channel ID", PropertyTypes.ChannelId),
                new Property("Fee Satoshi", PropertyTypes.ULong),
                new Property("Signature", PropertyTypes.Signature)
            }.AsReadOnly());
        
        public byte[] ChannelId { get; set; }
        public ulong FeeSatoshi { get; set; }
        public byte[] Signature { get; set; }
        
        public ClosingSignedMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            FeeSatoshi = propertyData[1].ToULongBigEndian();
            Signature = propertyData[2];
        }

        public override List<byte[]> GetProperties()
        {
            var list = new List<byte[]>
            {
                ChannelId,
                FeeSatoshi.GetBytesBigEndian(),
                Signature
            };
            
            return list;
        }

    }
}