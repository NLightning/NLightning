﻿using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.Network.GossipMessages
{
    public class AnnouncementSignaturesMessage : Message
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(259, typeof(AnnouncementSignaturesMessage),
            new List<Property> {
                new Property("Channel ID", PropertyTypes.ChannelId),
                new Property("Short Channel ID", PropertyTypes.ShortChannelId),
                new Property("Node Signature", PropertyTypes.Signature),
                new Property("Bitcoin Signature", PropertyTypes.Signature)
            }.AsReadOnly());
        
        public AnnouncementSignaturesMessage() : base(MessageDefinition)
        {
        }
        
        public byte[] ChannelId { get; set; }
        public byte[] ShortChannelId { get; set; }
        public byte[] NodeSignature { get; set; }
        public byte[] BitcoinSignature { get; set; }

        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            ShortChannelId = propertyData[1];
            NodeSignature = propertyData[2];
            BitcoinSignature = propertyData[3];
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>
            {
                ChannelId,
                ShortChannelId,
                NodeSignature,
                BitcoinSignature
            };
        }

    }
}