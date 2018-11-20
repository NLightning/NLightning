using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.Establishment.Messages
{
    public class FundingCreatedMessage : Message, ITemporaryChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(34, typeof(FundingCreatedMessage),
            new List<Property> {
                new Property("Temporary Channel ID", PropertyTypes.ChannelId),
                new Property("Funding Transaction ID", PropertyTypes.TransactionId),
                new Property("Funding Output Index", PropertyTypes.UShort),
                new Property("Signature", PropertyTypes.Signature)
            }.AsReadOnly());
        
        public byte[] TemporaryChannelId { get; set; }
        public byte[] FundingTransactionId { get; set; }
        public ushort FundingOutputIndex { get; set; }
        public byte[] Signature { get; set; }

       
        public FundingCreatedMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            TemporaryChannelId = propertyData[0];
            FundingTransactionId = propertyData[1];
            FundingOutputIndex = propertyData[2].ToUShortBigEndian();
            Signature = propertyData[3];
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>
            {
                TemporaryChannelId,
                FundingTransactionId,
                FundingOutputIndex.GetBytesBigEndian(),
                Signature
            };
        }

    }
}