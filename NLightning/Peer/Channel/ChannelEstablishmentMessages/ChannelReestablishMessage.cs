using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.ChannelEstablishmentMessages
{
    public class ChannelReestablishMessage : Message, IChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(136, typeof(ChannelReestablishMessage),
            new List<Property> {
                new Property("Channel ID", PropertyTypes.ChannelId),
                new Property("Next Local Commitment Number", PropertyTypes.ULong),
                new Property("Next Remote Revocation Number", PropertyTypes.ULong),
                new Property("Your Last Per Commitment Secret", PropertyTypes.PrivateKey, true),
                new Property("My Current Per Commitment Point", PropertyTypes.PublicKey, true)
            }.AsReadOnly());
        
        public byte[] ChannelId { get; set; }
        public ulong NextLocalCommitmentNumber { get; set; }
        public ulong NextRemoteRevocationNumber { get; set; }
        public ECKeyPair YourLastPerCommitmentSecret { get; set; }
        public ECKeyPair MyCurrentPerCommitmentPoint { get; set; }
        
        public ChannelReestablishMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            NextLocalCommitmentNumber = propertyData[1].ToULongBigEndian();
            NextRemoteRevocationNumber = propertyData[2].ToULongBigEndian();

            if (propertyData.Count == 5 && propertyData[3].Length == 32)
            {
                YourLastPerCommitmentSecret = new ECKeyPair(propertyData[3], true);
                MyCurrentPerCommitmentPoint = new ECKeyPair(propertyData[4], false);
            }
        }

        public override List<byte[]> GetProperties()
        {
            var list = new List<byte[]>
            {
                ChannelId,
                NextLocalCommitmentNumber.GetBytesBigEndian(),
                NextRemoteRevocationNumber.GetBytesBigEndian()
            };
            
            if (YourLastPerCommitmentSecret != null && MyCurrentPerCommitmentPoint != null && YourLastPerCommitmentSecret.HasPrivateKey)
            {
                list.Add(YourLastPerCommitmentSecret.PrivateKeyData);
                list.Add(MyCurrentPerCommitmentPoint.PublicKeyCompressed);
            }

            return list;
        }

    }
}