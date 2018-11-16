using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class ChannelDisabledMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(20, typeof(ChannelDisabledMessage), 
                new List<Property>()
                {
                    new Property("Flags", PropertyTypes.Flags2),
                    new Property("Channel Update", PropertyTypes.VariableArray)    
                }.AsReadOnly(), FailureCodes.Update);
        
        public ChannelDisabledMessage() : base(MessageDefinition)
        {
        }
        
        public byte[] Flags { get; set; }
        public byte[] ChannelUpdate { get; set; }

        public override void SetProperties(List<byte[]> propertyData)
        {
            Flags = propertyData[0];
            ChannelUpdate = propertyData[1];
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>()
            {
                Flags,
                ChannelUpdate
            };
        }
    }
}