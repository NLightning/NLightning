using System;
using System.Collections.ObjectModel;
using NLightning.Transport.Messaging;

namespace NLightning.OnionRouting
{
    public class OnionMessageDefinition : MessageDefinition
    {
        public OnionMessageDefinition(ushort typeId, Type type, ReadOnlyCollection<Property> properties, FailureCodes failureCodes)
            : base(typeId, type, properties)
        {
            FailureCodes = failureCodes;
        }
        
        public FailureCodes FailureCodes { get; }
    }
}