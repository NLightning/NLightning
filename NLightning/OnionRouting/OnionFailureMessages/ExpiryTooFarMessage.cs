using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class ExpiryTooFarMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(21, typeof(ExpiryTooFarMessage), 
                new List<Property>().AsReadOnly(), FailureCodes.None);
        
        public ExpiryTooFarMessage() : base(MessageDefinition)
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