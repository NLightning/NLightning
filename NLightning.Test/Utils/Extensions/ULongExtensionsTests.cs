using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Utils.Extensions
{
    public class ULongExtensions
    {
        [Fact]
        public void MSatToSatoshiTest()
        {
            Assert.Equal((ulong)1, ((ulong)1001).MSatToSatoshi());
            Assert.Equal((ulong)1, ((ulong)1999).MSatToSatoshi());
            Assert.Equal((ulong)250, ((ulong)250999).MSatToSatoshi());
            Assert.Equal((ulong)0, ((ulong)999).MSatToSatoshi());
        }
    }
}