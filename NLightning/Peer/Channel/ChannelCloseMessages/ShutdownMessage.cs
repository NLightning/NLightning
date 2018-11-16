using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.Peer.Channel.ChannelCloseMessages
{
    public class ShutdownMessage : Message, IChannelMessage
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(38, typeof(ShutdownMessage),
            new List<Property> {
                new Property("Channel ID", PropertyTypes.ChannelId),
                new Property("ScriptPubKey", PropertyTypes.VariableArray)
            }.AsReadOnly());
        
        public byte[] ChannelId { get; set; }
        public byte[] ScriptPubKey { get; set; }
        
        public ShutdownMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            ScriptPubKey = propertyData[1];
        }

        public override List<byte[]> GetProperties()
        {
            var list = new List<byte[]>
            {
                ChannelId,
                ScriptPubKey
            };
            
            return list;
        }

    }
}