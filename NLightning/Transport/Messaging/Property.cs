using System;

namespace NLightning.Transport.Messaging
{
    public class Property
    {
        public string Name { get; }
        public PropertyType Type { get; }
        public bool Optional { get; }
        
        public Property(String name, PropertyType type)
        {
            Name = name;
            Type = type;
        }
        
        public Property(String name, PropertyType type, bool optional)
        {
            Name = name;
            Type = type;
            Optional = optional;
        }
    }
}