using System.Security.Cryptography;
using NLightning.Utils.Extensions;

namespace NLightning.Cryptography
{
    public class HmacSha256
    {
        private static readonly HMACSHA256 Hmac = new HMACSHA256();
        
        public static (byte[], byte[]) ComputeHashes(byte[] key, byte[] data)
        {
            byte[] tempKey = ComputeHash(key, data);
            byte[] output1 = ComputeHash(tempKey, new byte[] {1});
            byte[] output2 = ComputeHash(tempKey, output1.ConcatToNewArray(new byte[] {2}));
            
            return (output1, output2);
        }

        public static byte[] ComputeHash(byte[] key, byte[] data)
        {
            Hmac.Key=key;
            return Hmac.ComputeHash(data);
        }
        
    }
}