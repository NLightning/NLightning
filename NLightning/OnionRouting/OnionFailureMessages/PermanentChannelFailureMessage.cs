using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class PermanentChannelFailureMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(8, typeof(PermanentChannelFailureMessage), 
                new List<Property>().AsReadOnly(), FailureCodes.PermanentFailure);
        
        public PermanentChannelFailureMessage() : base(MessageDefinition)
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