using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class FinalIncorrectCltvExpiryMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(18, typeof(FinalIncorrectCltvExpiryMessage), 
                new List<Property>()
                {
                    new Property("CLTV expiry", PropertyTypes.UInt)
                }.AsReadOnly(), FailureCodes.Update);
        
        public FinalIncorrectCltvExpiryMessage() : base(MessageDefinition)
        {
        }
        
        public uint CltvExpiry { get; set; }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            CltvExpiry = propertyData[0].ToUIntBigEndian();
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>()
            {
                CltvExpiry.GetBytesBigEndian()
            };
        }
    }
}