using System.Collections.Generic;
using NLightning.Utils;
using NLightning.Utils.Extensions;

namespace NLightning.Transport.Messaging.ControlMessages
{
    public class PingMessage : Message
    {
        public ushort PongDataLength { get; private set; }
        public ushort DataLength { get; private set; }

        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(18, typeof(PingMessage),
            new List<Property> {
                new Property("num_pong_bytes", PropertyTypes.UShort),
                new Property("ping_data", PropertyTypes.VariableArray)
            }.AsReadOnly());
       
        public PingMessage() : base(MessageDefinition)
        {
        }

        public PingMessage(ushort pongDataLength, ushort dataLength) : base(MessageDefinition)
        {
            PongDataLength = pongDataLength;
            DataLength = dataLength;
        }

        public override void SetProperties(List<byte[]> propertyData)
        {
            PongDataLength = propertyData[0].ToUShortBigEndian();
            DataLength = (ushort)propertyData[1].Length;
        }

        public override List<byte[]> GetProperties()
        {
            var properties = new List<byte[]>();
            properties.Add(PongDataLength.GetBytesBigEndian());
            properties.Add(new byte[DataLength]);
            return properties;
        }

    }
}