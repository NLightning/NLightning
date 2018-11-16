using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class FinalIncorrectHtlcAmountMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(19, typeof(FinalIncorrectHtlcAmountMessage), 
                new List<Property>()
                {
                    new Property("Incoming HTLC Amount", PropertyTypes.ULong)
                }.AsReadOnly(), FailureCodes.None);
        
        public FinalIncorrectHtlcAmountMessage() : base(MessageDefinition)
        {
        }
        
        public ulong HtlcAmount { get; set; }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            HtlcAmount = propertyData[0].ToUIntBigEndian();
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>()
            {
                HtlcAmount.GetBytesBigEndian()
            };
        }
    }
}