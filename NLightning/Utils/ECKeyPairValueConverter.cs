using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NLightning.Cryptography;

namespace NLightning.Utils
{
    public class ECKeyPairValueConverter : ValueConverter<ECKeyPair, byte[]>
    {
        private static readonly Expression<Func<byte[], ECKeyPair>> EncodeExpression = x => Decode(x);
        private static readonly Expression<Func<ECKeyPair, byte[]>> DecodeExpression = x => Encode(x);
        
        public ECKeyPairValueConverter()
            : base(DecodeExpression, EncodeExpression)
        { }

        private static byte[] Encode(ECKeyPair key)
        {
            return key.HasPrivateKey ? key.PrivateKeyData : key.PublicKeyCompressed;
        }
        
        private static ECKeyPair Decode(byte[] data)
        {
            return new ECKeyPair(data, data.Length != 33);
        }
    }
}