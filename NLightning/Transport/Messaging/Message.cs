using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLightning.Utils;
using NLightning.Utils.Extensions;

namespace NLightning.Transport.Messaging
{
    public abstract class Message
    {
        private readonly MessageDefinition _messageDefinition;
        
        public Message(MessageDefinition messageDefinition)
        {
            _messageDefinition = messageDefinition;
        }

        public MessageDefinition Definition => _messageDefinition;
        public abstract void SetProperties(List<byte[]> propertyData);
        public abstract List<byte[]> GetProperties();
        
        public void ParsePayload(byte[] payload, int offset = 0)
        {
            List<byte[]> data = new List<byte[]>();
            int position = offset;
            
            foreach (var property in _messageDefinition.Properties)
            {
                if (position != payload.Length)
                {
                    if (property.Type.VariableLength)
                    {
                        int length = payload.ToUShortBigEndian(position) * property.Type.LengthPerProperty;
                        data.Add(payload.SubArray(position + 2, length));
                        position += length + 2;
                    }
                    else
                    {
                        data.Add(payload.SubArray(position, property.Type.Length));
                        position += property.Type.Length;
                    }
                }
                else if (!property.Optional)
                {
                    throw new MessageException($"Invalid message {GetType().Name} payload size. Property: {property.Name}");
                }
            }

            if (position != payload.Length)
            {
                throw new MessageException(
                    $"Invalid message payload size. Should be {payload.Length} but is {position}");
            }
            
            SetProperties(data);
        }

        public byte[] GetBytes()
        {
            var properties = GetProperties();
            var propertyData = new List<byte[]>();
            int index = 0;
            
            ValidateProperties(properties);
            
            propertyData.Add(_messageDefinition.TypeId.GetBytesBigEndian());
            
            foreach (var property in properties)
            {
                var definition = _messageDefinition.Properties[index];
                if (definition.Type.VariableLength)
                {
                    ushort length = (ushort)((ushort)property.Length / definition.Type.LengthPerProperty);
                    propertyData.Add(ByteExtensions.Combine(length.GetBytesBigEndian(), property));
                }
                else
                {
                    if (property.Length != definition.Type.Length)
                    {
                        throw new ArgumentException("wrong property size");
                    }
                    
                    propertyData.Add(property);
                }
                
                index++;
            }

            return ByteExtensions.Combine(propertyData.ToArray());
        }

        private void ValidateProperties(List<byte[]> properties)
        {
            int propertyCountMin = _messageDefinition.Properties.Count(p => !p.Optional);
            
            if (properties.Count < propertyCountMin && properties.Count > _messageDefinition.Properties.Count)
            {
                throw new ArgumentException("wrong property count");
            }
        }

        public static Message Create(MessageDefinition messageDefinition)
        {
            return (Message)Activator.CreateInstance(messageDefinition.Type);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            try
            {
                stringBuilder.AppendLine($"{GetType().Name}({_messageDefinition.TypeId})");
                var properties = GetProperties();
                int index = 0;
                foreach (var property in properties)
                {
                    var definition = _messageDefinition.Properties[index];
                    var propertyName = $"{definition.Name} ({definition.Type.Name}, {property.Length}): ";
                    var paddedName = propertyName.PadRight(45, ' ');
                    var humanReadableProperty = definition.Type.HumanReadableValueSupplier(property);
                    
                    stringBuilder.AppendLine($"{paddedName}{humanReadableProperty}");
                    
                    index++;
                }
            }
            catch (Exception exception)
            {
                return $"Corrupted Message: {GetType().Name}. Parsing Exception: {exception}";
            }
           

            return stringBuilder.ToString();
        }
    }
}