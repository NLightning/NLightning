using System;
using System.Globalization;

namespace NLightning.Utils.Extensions
{
    public static class StringExtensions
    {
        public static byte[] HexToByteArray(this String hex)
        {
            if (hex.Length % 2 == 1)
            {
                throw new ArgumentException("Invalid hex string (odd length)");
            }
            
            byte[] data = new byte[hex.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hex.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data; 
        }
        
        public static int GetHexValue(char hex) {
            int value = (int)hex;
            return value - (value < 58 ? 48 : 55);
        }
    }
}