using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class IncorrectPaymentAmountMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(16, typeof(IncorrectPaymentAmountMessage), 
                new List<Property>().AsReadOnly(), FailureCodes.PermanentFailure);
        
        public IncorrectPaymentAmountMessage() : base(MessageDefinition)
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