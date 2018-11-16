using NLightning.Utils.Extensions;

namespace NLightning.OnionRouting
{
    public class OnionErrorMessage
    {
        public OnionErrorMessage(byte[] hmac, byte[] failureMessageData, byte[] pad)
        {
            Hmac = hmac;
            FailureMessageData = failureMessageData;
            Pad = pad;
        }
        
        public byte[] Hmac { get; }
        public byte[] FailureMessageData { get; }
        public byte[] Pad { get; }

        public byte[] GetData()
        {
            return ByteExtensions.Combine(
                Hmac,
                ((ushort) FailureMessageData.Length).GetBytesBigEndian(),
                FailureMessageData,
                ((ushort) Pad.Length).GetBytesBigEndian(),
                Pad);
        }
    }
}