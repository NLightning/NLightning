using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class UnknownPaymentHashMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(15, typeof(UnknownPaymentHashMessage), 
                new List<Property>().AsReadOnly(), FailureCodes.PermanentFailure);
        
        public UnknownPaymentHashMessage() : base(MessageDefinition)
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