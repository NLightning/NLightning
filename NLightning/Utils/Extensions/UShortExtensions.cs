using System;
using System.Linq;

namespace NLightning.Utils.Extensions
{
    public static class UShortExtensions
    {
        public static byte[] GetBytesBigEndian(this ushort value)
        {
            byte[] data = BitConverter.GetBytes(value);
            return BitConverter.IsLittleEndian ? data.Reverse().ToArray() : data;
        }
    }
}