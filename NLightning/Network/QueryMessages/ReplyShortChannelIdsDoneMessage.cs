﻿using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.Network.QueryMessages
{
    public class ReplyShortChannelIdsDoneMessage : Message
    {
        public byte[] ChainHash { get; private set; }

        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(262 , typeof(ReplyShortChannelIdsDoneMessage),
            new List<Property> {
                new Property("Chain Hash", PropertyTypes.Hash32),
                new Property("Completed", PropertyTypes.Byte)
            }.AsReadOnly());
        
        public ReplyShortChannelIdsDoneMessage() : base(MessageDefinition)
        {
        }
        
        public ReplyShortChannelIdsDoneMessage(byte[] chainHash) 
            : base(MessageDefinition)
        {
            ChainHash = chainHash;
 
        }

        public override void SetProperties(List<byte[]> propertyData)
        {

        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]> { ChainHash };
        }

    }
}