using System;
using System.Collections.ObjectModel;

namespace NLightning.Transport.Messaging
{
    public class MessageDefinition
    {
        public MessageDefinition(ushort typeId, Type type, ReadOnlyCollection<Property> properties)
        {
            TypeId = typeId;
            Type = type;
            Properties = properties;
        }

        public Type Type { get; }
        public ushort TypeId { get; }
        public ReadOnlyCollection<Property> Properties { get; }
    }
}