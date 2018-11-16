using System.IO;
using System.IO.Compression;

namespace NLightning.Utils
{
    public class ZLibCompression
    {
        public byte[] Compress(byte[] data)
        {
            using (MemoryStream inputStream = new MemoryStream(data))
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(outputStream, CompressionMode.Compress))
                {
                    inputStream.CopyTo(deflateStream);
                }
                
                return outputStream.ToArray();
            }
        }

        public byte[] Decompress(byte[] data)
        {
            using (MemoryStream inputStream = new MemoryStream(data))
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