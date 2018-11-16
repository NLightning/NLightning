using System.Collections.Generic;

namespace NLightning.Transport.Messaging.ControlMessages
{
    public class PongMessage : Message
    {
        public ushort DataLength { get; private set; }

        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(19, typeof(PongMessage),
            new List<Property> {
                new Property("pong_data", PropertyTypes.VariableArray)
            }.AsReadOnly());
       
        public PongMessage() : base(MessageDefinition)
        {
        }
        
        public PongMessage(ushort dataLength) : base(MessageDefinition)
        {
            DataLength = dataLength;
        }

        public override void SetProperties(List<byte[]> propertyData)
        {
            DataLength = (ushort)propertyData[0].Length;
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]> { new byte[DataLength] };
        }

    }
}