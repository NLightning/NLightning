using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class FinalExpiryTooSoonMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(17, typeof(FinalExpiryTooSoonMessage), 
                new List<Property>().AsReadOnly(), FailureCodes.None);
        
        public FinalExpiryTooSoonMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>() {};
        }
    }
}