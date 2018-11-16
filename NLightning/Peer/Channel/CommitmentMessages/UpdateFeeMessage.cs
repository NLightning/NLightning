using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.CommitmentMessages
{
    public class UpdateFeeMessage : Message, IChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(134, typeof(UpdateFeeMessage),
            new List<Property> {
                new Property("Channel ID", PropertyTypes.ChannelId),
                new Property("Feerate Per KW", PropertyTypes.UInt)
            }.AsReadOnly());
        
        public byte[] ChannelId { get; set; }
        public uint FeeratePerKw { get; set; }
        
        public UpdateFeeMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            FeeratePerKw = propertyData[1].ToUIntBigEndian();
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>
            {
                ChannelId,
                FeeratePerKw.GetBytesBigEndian()
            };
        }

    }
}