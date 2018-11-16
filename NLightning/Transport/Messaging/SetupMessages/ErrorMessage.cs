using System;
using System.Collections.Generic;
using System.Text;

namespace NLightning.Transport.Messaging.SetupMessages
{
    public class ErrorMessage : Message
    {
        public byte[] ChannelId { get; private set; }
        public String Data { get; private set; }

        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(17, typeof(ErrorMessage),
            new List<Property> {
                new Property("channelId", PropertyTypes.ChannelId),
                new Property("data", PropertyTypes.VariableArray)
            }.AsReadOnly());
        
        public ErrorMessage() : base(MessageDefinition)
        {
        }
        
        public ErrorMessage(byte[] channelId, String data) 
            : base(MessageDefinition)
        {
            ChannelId = channelId;
            Data = data;
        }

        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelId = propertyData[0];
            Data = Encoding.ASCII.GetString(propertyData[1]);
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]> {ChannelId, Encoding.ASCII.GetBytes(Data)};
        }

        public static ErrorMessage UnknownChannel(byte[] channelId)
        {
            return new ErrorMessage(channelId, "Unknown channel");
        }
    }
}