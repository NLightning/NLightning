using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NBitcoin;

namespace NLightning.Utils
{
    public class TransactionSignatureConverter : ValueConverter<TransactionSignature, byte[]>
    {
        private static readonly Expression<Func<byte[], TransactionSignature>> EncodeExpression = x => Decode(x);
        private static readonly Expression<Func<TransactionSignature, byte[]>> DecodeExpression = x => Encode(x);
        
        public TransactionSignatureConverter()
            : base(DecodeExpression, EncodeExpression)
        { }

        private static byte[] Encode(TransactionSignature signature)
        {
            return signature.ToBytes();
        }
        
        private static TransactionSignature Decode(byte[] data)
        {
            return new TransactionSignature(data);
        }
    }
}