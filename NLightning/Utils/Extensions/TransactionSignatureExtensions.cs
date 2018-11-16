using NBitcoin;

namespace NLightning.Utils.Extensions
{
    public static class TransactionSignatureExtensions
    {
        public static byte[] ToRawSignature(this TransactionSignature signature)
        {
            byte[] r = signature.Signature.R.ToByteArrayUnsigned();
            byte[] s = signature.Signature.S.ToByteArrayUnsigned();

            if (r.Length != 32)
            {
                r = ToFixedLength(r, 32);
            }
            
            if (s.Length != 32)
            {
                s = ToFixedLength(s, 32);
            }

            return ByteExtensions.Combine(r, s);
        }

        private static byte[] ToFixedLength(byte[] bytes, int length)
        {
            byte[] fixedLengthData = new byte[length];

            for (int i = 0; i < bytes.Length; i++)
            {
                fixedLengthData[i] = bytes[i];
            }

            return fixedLengthData;
        }
    }
}