﻿using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils;
using NLightning.Utils.Extensions;

namespace NLightning.Network.QueryMessages
{
    public class ReplyQueryChannelRangeMessage : Message
    {
        public byte[] ChainHash { get; private set; }
        public uint FirstBlockNumber { get;private set; }
        public uint NumberOfBlocks { get; private set; }
        public List<byte[]> ShortIds { get; private set; }
        public bool Complete { get; private set; }
        public bool CompressShortIds { get; set; } = false;
        
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(264 , typeof(ReplyQueryChannelRangeMessage),
            new List<Property> {
                new Property("Chain Hash", PropertyTypes.Hash32),
                new Property("First Block Number", PropertyTypes.UInt),
                new Property("Number of Blocks", PropertyTypes.UInt),
                new Property("Complete", PropertyTypes.Byte),
                new Property("Encoded Short IDs", PropertyTypes.VariableArray)
            }.AsReadOnly());
        
        public ReplyQueryChannelRangeMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            EncodedShortIdsEncoder shortIdsEncoder = new EncodedShortIdsEncoder();
            ChainHash = propertyData[0];
            FirstBlockNumber = propertyData[1].ToUIntBigEndian();
            NumberOfBlocks = propertyData[2].ToUIntBigEndian();
            Complete = propertyData[3][0] != 0;
            ShortIds = shortIdsEncoder.Decode(propertyData[4]);
            CompressShortIds = propertyData[4][0] != 0;
        }

        public override List<byte[]> GetProperties()
        {
            EncodedShortIdsEncoder shortIdsEncoder = new EncodedShortIdsEncoder();
            return new List<byte[]> { ChainHash, 
                FirstBlockNumber.GetBytesBigEndian(), 
                NumberOfBlocks.GetBytesBigEndian(),
                Complete ? new byte[] { 255 } : new byte[1],
                shortIdsEncoder.Encode(ShortIds, CompressShortIds)
            };
        }

    }
}