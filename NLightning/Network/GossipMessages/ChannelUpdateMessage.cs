﻿using System;
using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Network.GossipMessages
{
    public class ChannelUpdateMessage : Message
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(258, typeof(ChannelUpdateMessage),
            new List<Property> {
                new Property("Signature", PropertyTypes.Signature),
                new Property("Chain Hash", PropertyTypes.Hash32),
                new Property("Short Channel ID", PropertyTypes.ShortChannelId),
                new Property("Timestamp", PropertyTypes.Timestamp),
                new Property("Message Flags", PropertyTypes.Byte),
                new Property("Channel Flags", PropertyTypes.Byte),
                new Property("CLTV Expiry Delta", PropertyTypes.UShort),
                new Property("HTLC Minimum mSat", PropertyTypes.ULong),
                new Property("Fee Base mSat", PropertyTypes.UInt),
                new Property("Fee Proportional Millionths", PropertyTypes.UInt),
                new Property("HTLC Maximum mSat", PropertyTypes.ULong, true)
            }.AsReadOnly());
        
        public ChannelUpdateMessage() : base(MessageDefinition)
        {
        }
       
        public byte[] Signature { get; private set; }
        public byte[] ChainHash { get; private set;}
        public string ShortChannelIdHex { get; private set; }
        public byte[] ShortChannelId { get; private set; }
        public DateTime Timestamp { get; private set;}
        public byte MessageFlags { get; private set; }
        public byte ChannelFlags { get; private set; }
        public ushort CltvExpiryDelta { get; private set; }
        public ulong HtlcMinimumSat { get; private set; }
        public uint FeeBaseMsat { get; private set; }
        public uint FeeProportionalMillionths { get; private set; }
        public ulong HtlcMaximumSat { get; private set; }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            Signature = propertyData[0];
            ChainHash = propertyData[1];
            ShortChannelId = propertyData[2];
            ShortChannelIdHex = propertyData[2].ToHex();
            Timestamp = DateTimeExtensions.CreateFromUnixSeconds(propertyData[3].ToUIntBigEndian());
            MessageFlags = propertyData[4][0];
            ChannelFlags = propertyData[5][0];
            CltvExpiryDelta = propertyData[6].ToUShortBigEndian();
            HtlcMinimumSat = propertyData[7].ToULongBigEndian();
            FeeBaseMsat = propertyData[8].ToUIntBigEndian();
            FeeProportionalMillionths = propertyData[9].ToUIntBigEndian();

            if (propertyData.Count > 10)
            {
                HtlcMaximumSat = propertyData[10].ToULongBigEndian();
            }
        }

        public override List<byte[]> GetProperties()
        {
            var data = new List<byte[]>
            {
                Signature,
                ChainHash,
                ShortChannelId,
                Timestamp.ToUnixSeconds().GetBytesBigEndian(),
                new[] { MessageFlags },
                new[] { ChannelFlags },
                CltvExpiryDelta.GetBytesBigEndian(),
                HtlcMinimumSat.GetBytesBigEndian(),
                FeeBaseMsat.GetBytesBigEndian(),
                FeeProportionalMillionths.GetBytesBigEndian()
            };

            if (HtlcMaximumSat > 0)
            {
                data.Add(HtlcMaximumSat.GetBytesBigEndian());
            }

            return data;
        }
    }
}