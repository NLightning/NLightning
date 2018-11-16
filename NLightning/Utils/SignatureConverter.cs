using System;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.Crypto;
using NLightning.Utils.Extensions;

namespace NLightning.Utils
{
    public static class SignatureConverter
    {
        public static TransactionSignature RawToTransactionSignature(byte[] rawSignature)
        {
            if (rawSignature.Length != 64)
            {
                throw new ArgumentException("invalid signature length", nameof(rawSignature));
            }

            var r = new BigInteger(1, rawSignature.SubArray(0, 32));
            var s = new BigInteger(1, rawSignature.SubArray(32, 32));
            
            var ecdsaSig = new ECDSASignature(r, s).ToDER();
            return new TransactionSignature(ecdsaSig, SigHash.All);
        }
    }
}