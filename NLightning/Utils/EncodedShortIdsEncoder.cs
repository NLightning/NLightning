using System;
using System.Collections.Generic;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Utils
{
    public class EncodedShortIdsEncoder
    {
        public byte[] Encode(List<byte[]> shortIdsToEncode, bool compress)
        {
            var data = ByteExtensions.Combine(shortIdsToEncode.ToArray());

            if (compress)
            {
                ZLibCompression zLibCompression = new ZLibCompression();
                var compressed = zLibCompression.Compress(data);
                return ByteExtensions.Combine(new byte[] { 1 }, compressed);
            }

            return ByteExtensions.Combine(new byte[] { 0 }, data);
        }

        public List<byte[]> Decode(Span<byte> encoded)
        {
            Span<byte> data = encoded.Slice(1, encoded.Length - 1);
            Span<byte> decompressed;

            if (encoded[0] == 0)
            {
                decompressed = data;
            }
            else if (encoded[0] == 1)
            {
                ZLibCompression zLibCompression = new ZLibCompression();
                decompressed = zLibCompression.Decompress(data.ToArray());
            }
            else
            {
                throw new MessageNotSupportedException("Compression type not supported.");
            }

            List<byte[]> shortIds = new List<byte[]>();
            for (int i = 0; i < decompressed.Length; i = i + 8)
            {
                shortIds.Add(decompressed.Slice(i, 8).ToArray());
            }

            return shortIds;
        }
        
    }
}