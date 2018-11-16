using System;
using NLightning.Utils;
using NLightning.Utils.Extensions;

namespace NLightning.Cryptography
{
    public class Nonce
    {
        private long _value;
        
        public void Increment()
        {
            _value++;
        }
        
        public void Reset()
        {
            _value = 0;
        }
        
        public long Value => _value;
        public byte[] GetBytes() => ByteExtensions.Combine(BitConverter.GetBytes(0), BitConverter.GetBytes(_value));
    }
}