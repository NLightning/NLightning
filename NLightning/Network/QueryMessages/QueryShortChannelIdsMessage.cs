﻿using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils;

namespace NLightning.Network.QueryMessages
{
    public class QueryShortChannelIdsMessage : Message
    {
        public bool CompressShortIds { get; set; }
        public byte[] ChainHash { get; private set; }
        public List<byte[]> ShortChannelIds { get; private set; }
        
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(261 , typeof(QueryShortChannelIdsMessage),
            new List<Property> {
                new Property("Chain Hash", PropertyTypes.Hash32),
                new Property("Encoded Short IDs", PropertyTypes.VariableArray)
            }.AsReadOnly());
        
        public QueryShortChannelIdsMessage() : base(MessageDefinition)
        {
        }
        
        public QueryShortChannelIdsMessage(byte[] chainHash, List<byte[]> shortChannelIds, bool compressShortIds) 
            : base(MessageDefinition)
        {
            CompressShortIds = compressShortIds;
            ChainHash = chainHash;
            ShortChannelIds = shortChannelIds;
        }

        public override void SetProperties(List<byte[]> propertyData)
        {
            EncodedShortIdsEncoder shortIdsEncoder = new EncodedShortIdsEncoder();
            ChainHash = propertyData[0];
            ShortChannelIds = shortIdsEncoder.Decode(propertyData[1]);
            CompressShortIds = propertyData[1][0] != 0;
        }

        public override List<byte[]> GetProperties()
        {
            EncodedShortIdsEncoder shortIdsEncoder = new EncodedShortIdsEncoder();
            return new List<byte[]> { ChainHash, shortIdsEncoder.Encode(ShortChannelIds, CompressShortIds) };
        }

    }
}