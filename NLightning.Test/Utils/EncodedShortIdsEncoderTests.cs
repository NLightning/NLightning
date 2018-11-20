using System.Collections.Generic;
using System.Linq;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Utils
{
    public class EncodedShortIdsEncoderTests
    {
        [Fact]
        public void EncodeWithoutCompressionTest()
        {
            var encoder = new EncodedShortIdsEncoder();

            List<byte[]> ids = new List<byte[]>() { "13bc9c0000800000".HexToByteArray(),
                                                    "13bc9c0000801111".HexToByteArray(),
                                                    "13bc9c0000802222".HexToByteArray()
            };

            var encoded = encoder.Encode(ids, false);
            
            Assert.Equal("0013bc9c000080000013bc9c000080111113bc9c0000802222", encoded.ToHex());
        }
        
        [Fact]
        public void EncodeWithCompressionTest()
        {
            var encoder = new EncodedShortIdsEncoder();

            List<byte[]> ids = new List<byte[]>() { "13bc9c0000800000".HexToByteArray(),
                "13bc9c0000801111".HexToByteArray(),
                "13bc9c0000802222".HexToByteArray()
            };

            var encoded = encoder.Encode(ids, true);
            
            Assert.Equal("01789c13de338781a1818141184c0b0a4268252500", encoded.ToHex());
        }
        
                
        [Fact]
        public void DecodeWithoutCompressionTest()
        {
            var encoder = new EncodedShortIdsEncoder();

            List<byte[]> reference = new List<byte[]>() { "13bc9c0000800000".HexToByteArray(),
                "13bc9c0000801111".HexToByteArray(),
                "13bc9c0000802222".HexToByteArray()
            };

            var decoded = encoder.Decode("0013bc9c000080000013bc9c000080111113bc9c0000802222".HexToByteArray());
            
            Assert.Equal(reference.Count, decoded.Count);

            for (int i = 0; i < reference.Count; i++)
            {
                Assert.True(reference[i].SequenceEqual(decoded[i]));
            }
        }
        
        [Fact]
        public void DecodeWithCompressionTest()
        {
            var encoder = new EncodedShortIdsEncoder();

            List<byte[]> reference = new List<byte[]>() { "13bc9c0000800000".HexToByteArray(),
                "13bc9c0000801111".HexToByteArray(),
                "13bc9c0000802222".HexToByteArray()
            };

            var decoded = encoder.Decode("01780113de338781a1818141184c0b0a4268252500".HexToByteArray());
            
            Assert.Equal(reference.Count, decoded.Count);

            for (int i = 0; i < reference.Count; i++)
            {
                Assert.True(reference[i].SequenceEqual(decoded[i]));
            }
        }
    }
}