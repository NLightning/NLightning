using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class IncorrectCltvExpiryMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(13, typeof(IncorrectCltvExpiryMessage), 
                new List<Property>()
                {
                    new Property("CLTV Expiry", PropertyTypes.UInt),
                    new Property("Channel Update", PropertyTypes.VariableArray)
                }.AsReadOnly(), FailureCodes.Update);
        
        public IncorrectCltvExpiryMessage() : base(MessageDefinition)
        {
        }
        
        public uint CltvExpiry { get; set; }
        public byte[] ChannelUpdate { get; set; }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            CltvExpiry = propertyData[0].ToUIntBigEndian();
            ChannelUpdate = propertyData[1];
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>()
            {
                CltvExpiry.GetBytesBigEndian(),
                ChannelUpdate
            };
        }
    }
}