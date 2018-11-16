using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class TemporaryChannelFailureMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(7, typeof(TemporaryChannelFailureMessage), 
                new List<Property> {
                    new Property("Channel Update", PropertyTypes.VariableArray)
                }.AsReadOnly(), FailureCodes.Update);
        
        public TemporaryChannelFailureMessage() : base(MessageDefinition)
        {
        }
        
        public byte[] ChannelUpdate { get; set; }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelUpdate = propertyData[0];
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]> { ChannelUpdate };
        }
    }
}