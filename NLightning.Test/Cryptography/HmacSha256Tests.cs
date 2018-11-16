using System;
using NLightning.Cryptography;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Cryptography
{
    public class HmacSha256Tests
    {
        [Fact]
        public void GenerateHashesTest()
        {
            String salt = "2640f52eebcd9e882958951c794250eedb28002c05d7dc2ea0f195406042caf1";
            String data = "1e2fb3c8fe8fb9f262f649f64d26ecf0f2c0a805a767cf02dc2d77a6ef1fdcc3";

            (byte[] output1, byte[] output2) = HmacSha256.ComputeHashes(salt.HexToByteArray(), data.HexToByteArray());

            String output1Expected = "b61ec1191326fa240decc9564369dbb3ae2b34341d1e11ad64ed89f89180582f";
            String output2Expected = "e68f69b7f096d7917245f5e5cf8ae1595febe4d4644333c99f9c4a1282031c9f";
            
            Assert.Equal(output1Expected, output1.ToHex());
            Assert.Equal(output2Expected, output2.ToHex());
        }
    }
}