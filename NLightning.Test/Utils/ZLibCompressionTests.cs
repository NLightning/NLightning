using System;
using System.Text;
using NLightning.Utils;
using Xunit;

namespace NLightning.Test.Utils
{
    public class ZLibCompressionTests
    {
        [Fact]
        public void CompressAndDecompressTest()
        {
            String reference = "Hello World 0123456789";
            
            byte[] dataToCompress = Encoding.Unicode.GetBytes(reference);
            ZLibCompression zLibCompression = new ZLibCompression();

            var compressed = zLibCompression.Compress(dataToCompress);
            var decompressed = zLibCompression.Decompress(compressed);
            
            Assert.Equal(reference, Encoding.Unicode.GetString(decompressed));
        }
    }
}