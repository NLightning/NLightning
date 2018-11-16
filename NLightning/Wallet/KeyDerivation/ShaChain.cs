using System;
using System.Collections;
using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Utils.Extensions;

namespace NLightning.Wallet.KeyDerivation
{
    public class ShaChain
    {
        private readonly byte[] _root;

        public ShaChain(byte[] root)
        {
            _root = root;
        }
        
        public byte[] Derive(ulong index)
        {
            var positions = DeriveBitTransformations(0, index);
            byte[] buffer = new byte[_root.Length];
            Array.Copy(_root, buffer, _root.Length);

            foreach (var position in positions)
            {
                var byteNumber = position / 8;
                var bitNumber = position % 8;
                buffer[byteNumber] ^= (byte)(1 << bitNumber);
                buffer = SHA256.ComputeHash(buffer);
            }
            
            return buffer;
        }
        
        public byte[] DeriveBitTransformations(ulong from, ulong to)
        {
            List<byte> positions = new List<byte>();
            if (from == to)
            {
                return new byte[0];
            }

            byte zeros = CountTrailingBinaryZeros(from);
            if (!CanDerive(from, to, zeros)) {
                throw new InvalidOperationException("Can't derive (different indexes)");
            }

            BitArray bitArray = new BitArray(to.GetBytesLittleEndian());
            for (short i = (byte)(zeros - 1); i >= 0; i--) {
                if (bitArray.Get(i))
                {
                    positions.Add((byte)i);
                }
            }

            return positions.ToArray();
        }

        private bool CanDerive(ulong from, ulong to, byte zeros)
        {
            ulong mask = (ulong)((1 << zeros) - 1);
            return from == (to & mask);
        }
       
        private static byte CountTrailingBinaryZeros(ulong number)
        {
            ulong mask = 1;
            for (byte i = 0; i < 64; i++, mask <<= 1)
            {
                if ((number & mask) != 0)
                {
                    return i;
                }
            }

            return 64;
        }
    }
}