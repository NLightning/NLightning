using NLightning.Transport.Messaging.ControlMessages;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Transport.Messaging.ControlMessages
{
    public class PongTests
    {
        [Fact]
        public void GetBytesTest()
        {
            PongMessage message = new PongMessage(12);

            byte[] actual = message.GetBytes();
            Assert.Equal("0013000c000000000000000000000000", actual.ToHex());
        }
        
        [Fact]
        public void ParseTest()
        {
            PongMessage message = new PongMessage();
            message.ParsePayload("000c000000000000000000000000".HexToByteArray());
            
            Assert.Equal(12, message.DataLength);
        }
    }
}