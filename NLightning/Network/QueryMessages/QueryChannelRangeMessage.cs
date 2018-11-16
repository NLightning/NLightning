﻿using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils;
using NLightning.Utils.Extensions;

namespace NLightning.Network.QueryMessages
{
    public class QueryChannelRangeMessage : Message
    {
        public byte[] ChainHash { get; private set; }
        public uint FirstBlockNumber { get;private set; }
        public uint NumberOfBlocks { get;private set; }

        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(263 , typeof(QueryChannelRangeMessage),
            new List<Property> {
                new Property("Chain Hash", PropertyTypes.Hash32),
                new Property("First Block Number", PropertyTypes.UInt),
                new Property("Number of Blocks", PropertyTypes.UInt)
            }.AsReadOnly());
            
        public QueryChannelRangeMessage() : base(MessageDefinition)
        {
        }
        
        public QueryChannelRangeMessage(byte[] chainHash, uint firstBlockNumber, uint numberOfBlocks) 
            : base(MessageDefinition)
        {
            ChainHash = chainHash;
            FirstBlockNumber = firstBlockNumber;
            NumberOfBlocks = numberOfBlocks;
        }

        public override void SetProperties(List<byte[]> propertyData)
        {
            ChainHash = propertyData[0];
            FirstBlockNumber = propertyData[1].ToUIntBigEndian();
            NumberOfBlocks = propertyData[2].ToUIntBigEndian();
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]> { ChainHash, FirstBlockNumber.GetBytesBigEndian(), NumberOfBlocks.GetBytesBigEndian() };
        }

    }
}