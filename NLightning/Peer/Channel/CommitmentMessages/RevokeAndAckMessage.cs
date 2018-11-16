using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.CommitmentMessages
{
    public class RevokeAndAckMessage : Message, IChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(134, typeof(RevokeAndAckMessage),
            new List<Property> {
                new Property("Channel ID", PropertyTypes.ChannelId),
                new Property("Per Commitment Secret", PropertyTypes.PrivateKey),
                new Property("Next Per Commitment Point", PropertyTypes.PublicKey)
            }.AsReadOnly());
        
        public byte[] ChannelId { get; set; }
        public ECKeyPair PerCommitmentSecret { get; set; }
        public ECKeyPair NextPerCommitmentPoint { get; set; }
        
        public RevokeAndAckMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            PerCommitmentSecret = new ECKeyPair(propertyData[1], true);
            NextPerCommitmentPoint = new ECKeyPair(propertyData[2], false);
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>
            {
                ChannelId,
                PerCommitmentSecret.PrivateKeyData,
                NextPerCommitmentPoint.PublicKeyCompressed
            };
        }

    }
}