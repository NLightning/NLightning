using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel.Establishment.Messages
{
    public class AcceptChannelMessage : Message, ITemporaryChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(33, typeof(AcceptChannelMessage),
            new List<Property> {
                new Property("Temporary Channel ID", PropertyTypes.ChannelId),
                new Property("Dust Limit Satoshis", PropertyTypes.ULong),
                new Property("Max HTLC Value In Flight mSat", PropertyTypes.ULong),
                new Property("Channel Reserve Satoshis", PropertyTypes.ULong),
                new Property("HTLC Minimum mSat", PropertyTypes.ULong),
                new Property("Minimum Depth", PropertyTypes.UInt),
                new Property("To Self Delay", PropertyTypes.UShort),
                new Property("Max Accepted HTLCs", PropertyTypes.UShort),
                new Property("Funding PubKey", PropertyTypes.PublicKey),
                new Property("Revocation Basepoint", PropertyTypes.PublicKey),
                new Property("Payment Basepoint", PropertyTypes.PublicKey),
                new Property("Delayed Payment Basepoint", PropertyTypes.PublicKey),
                new Property("HTLC Basepoint", PropertyTypes.PublicKey),
                new Property("First Per Commitment Point", PropertyTypes.PublicKey),
                new Property("Shutdown ScriptPubKey", PropertyTypes.VariableArray, true)
            }.AsReadOnly());
        
        public byte[] TemporaryChannelId { get; set; }
        public ulong DustLimitSatoshis { get; set; }
        public ulong MaxHtlcValueInFlightMSat { get; set; }
        public ulong ChannelReserveSatoshis { get; set; }
        public ulong HtlcMinimumMSat { get; set; }
        public uint MinimumDepth { get; set; }
        public ushort ToSelfDelay { get; set; }
        public ushort MaxAcceptedHtlcs { get; set; }
        public ECKeyPair FundingPubKey { get; set; }
        public ECKeyPair RevocationBasepoint { get; set; }
        public ECKeyPair PaymentBasepoint { get; set; }
        public ECKeyPair DelayedPaymentBasepoint { get; set; }
        public ECKeyPair HtlcBasepoint { get; set; }
        public ECKeyPair FirstPerCommitmentPoint { get; set; }
        public byte[] ShutdownScriptPubKey { get; set; }
        
        public AcceptChannelMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            TemporaryChannelId = propertyData[0];
            DustLimitSatoshis = propertyData[1].ToULongBigEndian();
            MaxHtlcValueInFlightMSat = propertyData[2].ToULongBigEndian();
            ChannelReserveSatoshis = propertyData[3].ToULongBigEndian();
            HtlcMinimumMSat = propertyData[4].ToULongBigEndian();
            MinimumDepth = propertyData[5].ToUIntBigEndian();
            ToSelfDelay = propertyData[6].ToUShortBigEndian();
            MaxAcceptedHtlcs = propertyData[7].ToUShortBigEndian();
            FundingPubKey = new ECKeyPair(propertyData[8], false);
            RevocationBasepoint = new ECKeyPair(propertyData[9], false);
            PaymentBasepoint = new ECKeyPair(propertyData[10], false);
            DelayedPaymentBasepoint = new ECKeyPair(propertyData[11], false);
            HtlcBasepoint = new ECKeyPair(propertyData[12], false);
            FirstPerCommitmentPoint = new ECKeyPair(propertyData[13], false);

            if (propertyData.Count > 14)
            {
                ShutdownScriptPubKey = propertyData[14];
            }
        }

        public override List<byte[]> GetProperties()
        {
            var list = new List<byte[]>
            {
                TemporaryChannelId,
                DustLimitSatoshis.GetBytesBigEndian(),
                MaxHtlcValueInFlightMSat.GetBytesBigEndian(),
                ChannelReserveSatoshis.GetBytesBigEndian(),
                HtlcMinimumMSat.GetBytesBigEndian(),
                MinimumDepth.GetBytesBigEndian(),
                ToSelfDelay.GetBytesBigEndian(),
                MaxAcceptedHtlcs.GetBytesBigEndian(),
                FundingPubKey.PublicKeyCompressed,
                RevocationBasepoint.PublicKeyCompressed,
                PaymentBasepoint.PublicKeyCompressed,
                DelayedPaymentBasepoint.PublicKeyCompressed,
                HtlcBasepoint.PublicKeyCompressed,
                FirstPerCommitmentPoint.PublicKeyCompressed
            };

            if (ShutdownScriptPubKey != null)
            {
                list.Add(ShutdownScriptPubKey);
            }
            
            return list;
        }

    }
}