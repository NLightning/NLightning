using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class RequiredNodeFeatureMissingMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(3, 
                typeof(RequiredNodeFeatureMissingMessage), new List<Property>().AsReadOnly(),
                FailureCodes.PermanentFailure | FailureCodes.NodeFailure);
        
        public RequiredNodeFeatureMissingMessage() : base(MessageDefinition)
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