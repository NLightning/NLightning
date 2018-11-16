using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class RequiredChannelFeatureMissingMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(9, typeof(RequiredChannelFeatureMissingMessage), 
                new List<Property>().AsReadOnly(), FailureCodes.PermanentFailure);
        
        public RequiredChannelFeatureMissingMessage() : base(MessageDefinition)
        {
        }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>();
        }
    }
}