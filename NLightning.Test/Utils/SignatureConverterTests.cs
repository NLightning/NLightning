using NLightning.Utils;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Utils
{
    public class SignatureConverterTests
    {
        [Fact]
        public void RawSignatureToTransactionSignatureTest()
        {
            byte[] rawSignature = "310e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464456452".HexToByteArray();

            var result = SignatureConverter.RawToTransactionSignature(rawSignature);
            var data = result.ToRawSignature();
            
            Assert.Equal(rawSignature.ToHex(), data.ToHex());
        }
    }
}