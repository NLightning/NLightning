using System.Collections.Generic;
using NLightning.Transport.Messaging;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class InvalidOnionHmacMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(5, typeof(InvalidOnionHmacMessage), 
                new List<Property> {
                    new Property("SHA256 of onion", PropertyTypes.Hash32)
                }.AsReadOnly(), FailureCodes.PermanentFailure | FailureCodes.BadOnion);
        
        public InvalidOnionHmacMessage() : base(MessageDefinition)
        {
        }
        
        public byte[] Sha256OfOnion { get; set; }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            Sha256OfOnion = propertyData[0];
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]> { Sha256OfOnion };
        }
    }
}