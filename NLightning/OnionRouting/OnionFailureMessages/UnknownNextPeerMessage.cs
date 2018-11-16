using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class UnknownNextPeerMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(10, typeof(UnknownNextPeerMessage), 
                new List<Property>().AsReadOnly(), FailureCodes.PermanentFailure);
        
        public UnknownNextPeerMessage() : base(MessageDefinition)
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