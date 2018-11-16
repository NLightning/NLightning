using System;
using System.Linq;

namespace NLightning.Utils.Extensions
{
    public static class ULongExtensions
    {
        public static byte[] GetBytesBigEndian(this ulong value)
        {
            byte[] data = BitConverter.GetBytes(value);
            return BitConverter.IsLittleEndian ? data.Reverse().ToArray() : data;
        }

        public static byte[] GetBytesLittleEndian(this ulong value)
        {
            byte[] data = BitConverter.GetBytes(value);
            return !BitConverter.IsLittleEndian ? data.Reverse().ToArray() : data;
        }

        public static ulong MSatToSatoshi(this ulong value)
        {
            double satoshi = value / 1000;
            return (ulong)Math.Ceiling(satoshi);
        }
        
        public static ulong CheckedSubtract(this ulong value, ulong add)
        {
            try
            {
                checked
                {
                    return value - add;
                }
            }
            catch (ArithmeticException)
            {
                return 0;
            }
        }
    }
}