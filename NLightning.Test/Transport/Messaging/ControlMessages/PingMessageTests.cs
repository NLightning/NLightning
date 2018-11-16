using NLightning.Transport.Messaging.ControlMessages;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Transport.Messaging.ControlMessages
{
    public class PingMessageTests
    {
        [Fact]
        public void GetBytesTest()
        {
            PingMessage message = new PingMessage(5, 10);

            byte[] actual = message.GetBytes();
            Assert.Equal("00120005000a00000000000000000000", actual.ToHex());
        }
        
        [Fact]
        public void ParseTest()
        {
            PingMessage message = new PingMessage();
            message.ParsePayload("0005000a00000000000000000000".HexToByteArray());
            
            Assert.Equal(5, message.PongDataLength);
            Assert.Equal(10, message.DataLength);
        }
    }
}