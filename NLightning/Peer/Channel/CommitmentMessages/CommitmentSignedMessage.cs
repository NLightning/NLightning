using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.CommitmentMessages
{
    public class CommitmentSignedMessage : Message, IChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(132, typeof(CommitmentSignedMessage),
            new List<Property> {
                new Property("Channel ID", PropertyTypes.ChannelId),
                new Property("Signature", PropertyTypes.Signature),
                new Property("HTLC signatures", PropertyTypes.Signatures)
            }.AsReadOnly());
        
        public byte[] ChannelId { get; set; }
        public byte[] Signature { get; set; }
        public List<byte[]> HtlcSignatures  { get; set; }
        
        public CommitmentSignedMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            Signature = propertyData[1];

            int signatureCount = propertyData[2].Length / 64;
            List<byte[]> signatures = new List<byte[]>();
            for (int i = 0; i < signatureCount; i++)
            {
                HtlcSignatures.Add(propertyData[2].SubArray(i*64, 64));    
            }
            
            HtlcSignatures = signatures;
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>
            {
                ChannelId,
                Signature,
                ByteExtensions.Combine(HtlcSignatures.ToArray())
            };
        }

    }
}