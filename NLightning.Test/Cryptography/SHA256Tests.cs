using NLightning.Cryptography;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Cryptography
{
    public class SHA256Tests
    {
        [Fact]
        public void ComputeHashTest()
        {
            Assert.Equal("e6f48a0036f29213687545ad901eb55949d15e150213f2db8b32f248d55ec411", SHA256.ComputeHash("1111111111111111".HexToByteArray()).ToHex());
        }
    }
}