using System;
using NLightning.Utils.Extensions;

namespace NLightning.Transport.Messaging
{
    public class PropertyType
    {
        public string Name { get; }
        public Type Type { get; }
        public short Length { get; }
        public bool VariableLength { get; }
        public ushort LengthPerProperty { get; set; } = 1;
        public Func<byte[], string> HumanReadableValueSupplier { get; }

        public PropertyType(String name, Type type, Func<byte[], string> humanReadableSupplier = null)
        {
            Name = name;
            Type = type;
            Length = -1;
            VariableLength = true;
            HumanReadableValueSupplier = humanReadableSupplier ?? (bytes => bytes.ToHex());
        }
        
        public PropertyType(String name, Type type, short length, Func<byte[], string> humanReadableSupplier = null)
        {
            Name = name;
            Type = type;
            Length = length;
            VariableLength = false;
            HumanReadableValueSupplier = humanReadableSupplier ?? (bytes => $"0x{bytes.ToHex()}");
        }
        
    }
}