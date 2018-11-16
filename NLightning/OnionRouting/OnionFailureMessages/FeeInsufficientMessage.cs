using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.OnionRouting.OnionFailureMessages
{
    public class FeeInsufficientMessage : Message
    {
        public static readonly OnionMessageDefinition MessageDefinition = 
            new OnionMessageDefinition(12, typeof(FeeInsufficientMessage), 
                new List<Property>()
                {
                    new Property("HTLC MSat", PropertyTypes.ULong),
                    new Property("Channel Update", PropertyTypes.VariableArray)
                }.AsReadOnly(), FailureCodes.Update);
        
        public FeeInsufficientMessage() : base(MessageDefinition)
        {
        }
        
        public ulong AmountMsat { get; set; }
        public byte[] ChannelUpdate { get; set; }
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            AmountMsat = propertyData[0].ToULongBigEndian();
            ChannelUpdate = propertyData[1];
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>()
            {
                AmountMsat.GetBytesBigEndian(),
                ChannelUpdate
            };
        }
    }
}