using System;
using System.Linq;

namespace NLightning.Utils.Extensions
{
    public static class UIntExtensions
    {
        public static byte[] GetBytesBigEndian(this uint value)
        {
            byte[] data = BitConverter.GetBytes(value);
            return BitConverter.IsLittleEndian ? data.Reverse().ToArray() : data;
        }
    }
}