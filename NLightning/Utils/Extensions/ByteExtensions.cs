using System;
using System.Linq;

namespace NLightning.Utils.Extensions
{
    public static class ByteExtensions
    {        
        public static ushort ToUShortBigEndian(this byte[] data, int index = 0)
        {
            return BitConverter.ToUInt16(data.SubArray(index, 2).Reverse().ToArray());
        }
       
        public static uint ToUIntBigEndian(this byte[] data, int index = 0)
        {
            return BitConverter.ToUInt32(data.SubArray(index, 4).Reverse().ToArray());
        }
        
        public static ulong ToULongBigEndian(this byte[] data, int index = 0)
        {
            return BitConverter.ToUInt64(data.SubArray(index, 8).Reverse().ToArray());
        }
        
        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays) {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
        
        public static String ToHex(this byte[] data)
        {
            char[] c = new char[data.Length * 2];
            byte b;
            for (int i = 0; i < data.Length; ++i)
            {
                b = (byte)(data[i] >> 4);
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = (byte)(data[i] & 0xF);
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }
            return new string(c).ToLower();
        }
    }
}