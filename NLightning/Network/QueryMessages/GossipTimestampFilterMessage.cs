﻿using System;
using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils;
using NLightning.Utils.Extensions;

namespace NLightning.Network.QueryMessages
{
    public class GossipTimestampFilterMessage : Message
    {
        public byte[] ChainHash { get; private set; }
        public DateTime StartDate { get; private set; }
        public TimeSpan Range { get; private set; }

        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(265, typeof(GossipTimestampFilterMessage),
            new List<Property> {
                new Property("Chain Hash", PropertyTypes.Hash32),
                new Property("First Timestamp", PropertyTypes.Timestamp),
                new Property("Timestamp Range", PropertyTypes.UInt)
            }.AsReadOnly());
        
        public GossipTimestampFilterMessage() : base(MessageDefinition)
        {
        }
        
        public GossipTimestampFilterMessage(byte[] chainHash, DateTime startDate, TimeSpan range) 
            : base(MessageDefinition)
        {
            ChainHash = chainHash;
            StartDate = startDate;
            Range = range;
        }

        public override void SetProperties(List<byte[]> propertyData)
        {
            ChainHash = propertyData[0];
            StartDate = DateTimeExtensions.CreateFromUnixSeconds(propertyData[1].ToUIntBigEndian());
            Range = TimeSpan.FromSeconds(propertyData[2].ToUIntBigEndian());
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]> { ChainHash, StartDate.ToUnixSeconds().GetBytesBigEndian(), ((uint)Range.Seconds).GetBytesBigEndian() };
        }

    }
}