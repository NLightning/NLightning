using System.IO;
using System.IO.Compression;
using NLightning.Utils.Extensions;

namespace NLightning.Utils
{
    public class ZLibCompression
    {
        private static readonly byte[] ZlibDefaultCompressionHeader = "789C".HexToByteArray();
        
        public byte[] Compress(byte[] data)
        {
            using (MemoryStream inputStream = new MemoryStream(data))
            using (MemoryStream outputStream = new MemoryStream())
            {
                outputStream.Write(ZlibDefaultCompressionHeader);
                
                using (DeflateStream deflateStream = new DeflateStream(outputStream, CompressionMode.Compress))
                {
                    inputStream.CopyTo(deflateStream);
                }

                return outputStream.ToArray();
            }
        }

        public byte[] Decompress(byte[] data)
        {
            using (MemoryStream inputStream = new MemoryStream(data, 2, data.Length - 2))
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (DeflateStream decompressionStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(outputStream);
                }
                
                return outputStream.ToArray();
            }
        }
    }
}