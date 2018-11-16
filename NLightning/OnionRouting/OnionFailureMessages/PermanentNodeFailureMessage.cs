using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class PermanentNodeFailureMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(2, typeof(PermanentNodeFailureMessage), new List<Property>().AsReadOnly(),
                FailureCodes.PermanentFailure | FailureCodes.NodeFailure);
        
        public PermanentNodeFailureMessage() : base(MessageDefinition)
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