using System.Collections.Generic;

namespace NLightning.Transport.Messaging.SetupMessages
{
    public class InitMessage : Message
    {
        public byte[] Globalfeatures { get; private set; }
        public byte[] Localfeatures { get; private set; }

        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(16, typeof(InitMessage),
            new List<Property> {
                new Property("global features", PropertyTypes.VariableArray),
                new Property("local features", PropertyTypes.VariableArray)
            }.AsReadOnly());
        
        public InitMessage() : base(MessageDefinition)
        {
        }
        
        public InitMessage(byte[] globalfeatures, byte[] localfeatures) 
            : base(MessageDefinition)
        {
            Globalfeatures = globalfeatures;
            Localfeatures = localfeatures;
        }

        public override void SetProperties(List<byte[]> propertyData)
        {
            Globalfeatures = propertyData[0];
            Localfeatures = propertyData[1];
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]> {Globalfeatures, Localfeatures};
        }

    }
}