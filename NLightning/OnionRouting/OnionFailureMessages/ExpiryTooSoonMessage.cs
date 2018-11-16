using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class ExpiryTooSoonMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(14, typeof(ExpiryTooSoonMessage), 
                new List<Property>()
                {
                    new Property("Channel Update", PropertyTypes.VariableArray)
                }.AsReadOnly(), FailureCodes.Update);
        
        public ExpiryTooSoonMessage() : base(MessageDefinition)
        {
        }
        
        public byte[] ChannelUpdate { get; set; }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            ChannelUpdate = propertyData[0];
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>()
            {
                ChannelUpdate
            };
        }
    }
}