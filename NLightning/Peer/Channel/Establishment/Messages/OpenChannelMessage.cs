using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.Establishment.Messages
{
    public class OpenChannelMessage : Message, ITemporaryChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(32, typeof(OpenChannelMessage),
            new List<Property> {
                new Property("Chain Hash", PropertyTypes.Hash32),
                new Property("Temporary Channel ID", PropertyTypes.ChannelId),
                new Property("Funding Satoshis", PropertyTypes.ULong),
                new Property("Push mSat", PropertyTypes.ULong),
                new Property("Dust Limit Satoshis", PropertyTypes.ULong),
                new Property("Max HTLC Value In Flight mSat", PropertyTypes.ULong),
                new Property("Channel Reserve Satoshis", PropertyTypes.ULong),
                new Property("HTLC Minimum mSat", PropertyTypes.ULong),
                new Property("Feerate Per KW", PropertyTypes.UInt),
                new Property("To Self Delay", PropertyTypes.UShort),
                new Property("Max Accepted HTLCs", PropertyTypes.UShort),
                new Property("Funding PubKey", PropertyTypes.PublicKey),
                new Property("Revocation Basepoint", PropertyTypes.PublicKey),
                new Property("Payment Basepoint", PropertyTypes.PublicKey),
                new Property("Delayed Payment Basepoint", PropertyTypes.PublicKey),
                new Property("HTLC Basepoint", PropertyTypes.PublicKey),
                new Property("First Per Commitment Point", PropertyTypes.PublicKey),
                new Property("Channel Flags", PropertyTypes.Byte),
                new Property("Shutdown ScriptPubKey", PropertyTypes.VariableArray, true)
            }.AsReadOnly());
        
        public byte[] ChainHash { get; set;}
        public byte[] TemporaryChannelId { get; set; }
        public ulong FundingSatoshis { get; set; }
        public ulong PushMSat { get; set; }
        public ulong DustLimitSatoshis { get; set; }
        public ulong MaxHtlcValueInFlightMSat { get; set; }
        public ulong ChannelReserveSatoshis { get; set; }
        public ulong HtlcMinimumMSat { get; set; }
        public uint FeeratePerKw { get; set; }
        public ushort ToSelfDelay { get; set; }
        public ushort MaxAcceptedHtlcs { get; set; }
        public ECKeyPair FundingPubKey { get; set; }
        public ECKeyPair RevocationBasepoint { get; set; }
        public ECKeyPair PaymentBasepoint { get; set; }
        public ECKeyPair DelayedPaymentBasepoint { get; set; }
        public ECKeyPair HtlcBasepoint { get; set; }
        public ECKeyPair FirstPerCommitmentPoint { get; set; }
        public byte ChannelFlags { get; set; }
        public byte[] ShutdownScriptPubKey { get; set; }
        
        public OpenChannelMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChainHash = propertyData[0];
            TemporaryChannelId = propertyData[1];
            FundingSatoshis = propertyData[2].ToULongBigEndian();
            PushMSat = propertyData[3].ToULongBigEndian();
            DustLimitSatoshis = propertyData[4].ToULongBigEndian();
            MaxHtlcValueInFlightMSat = propertyData[5].ToULongBigEndian();
            ChannelReserveSatoshis = propertyData[6].ToULongBigEndian();
            HtlcMinimumMSat = propertyData[7].ToULongBigEndian();
            FeeratePerKw = propertyData[8].ToUIntBigEndian();
            ToSelfDelay = propertyData[9].ToUShortBigEndian();
            MaxAcceptedHtlcs = propertyData[10].ToUShortBigEndian();
            FundingPubKey = new ECKeyPair(propertyData[11], false);
            RevocationBasepoint = new ECKeyPair(propertyData[12], false);
            PaymentBasepoint = new ECKeyPair(propertyData[13], false);
            DelayedPaymentBasepoint = new ECKeyPair(propertyData[14], false);
            HtlcBasepoint = new ECKeyPair(propertyData[15], false);
            FirstPerCommitmentPoint = new ECKeyPair(propertyData[16], false);
            ChannelFlags = propertyData[17][0];

            if (propertyData.Count > 18)
            {
                ShutdownScriptPubKey = propertyData[18];
            }
        }

        public override List<byte[]> GetProperties()
        {
            var list = new List<byte[]>
            {
                ChainHash,
                TemporaryChannelId,
                FundingSatoshis.GetBytesBigEndian(),
                PushMSat.GetBytesBigEndian(),
                DustLimitSatoshis.GetBytesBigEndian(),
                MaxHtlcValueInFlightMSat.GetBytesBigEndian(),
                ChannelReserveSatoshis.GetBytesBigEndian(),
                HtlcMinimumMSat.GetBytesBigEndian(),
                FeeratePerKw.GetBytesBigEndian(),
                ToSelfDelay.GetBytesBigEndian(),
                MaxAcceptedHtlcs.GetBytesBigEndian(),
                FundingPubKey.PublicKeyCompressed,
                RevocationBasepoint.PublicKeyCompressed,
                PaymentBasepoint.PublicKeyCompressed,
                DelayedPaymentBasepoint.PublicKeyCompressed,
                HtlcBasepoint.PublicKeyCompressed,
                FirstPerCommitmentPoint.PublicKeyCompressed,
                new[] { ChannelFlags }
            };

            if (ShutdownScriptPubKey != null)
            {
                list.Add(ShutdownScriptPubKey);
            }
            
            return list;
        }

    }
}