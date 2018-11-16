using System;

namespace NLightning.OnionRouting
{
    [Flags]
    public enum FailureCodes : ushort
    {
        None = 0,
        BadOnion = 0x8000,
        PermanentFailure = 0x4000,
        NodeFailure = 0x2000,
        Update = 0x1000,
    }
}