using System;
using NLightning.Transport.Messaging.SetupMessages;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Transport.Messaging.SetupMessages
{
    public class ErrorMessageTests
    {
        [Fact]
        public void GetBytesTest()
        {
            byte[] channelId = new byte[32];
            channelId[31] = 42;
            
            ErrorMessage message = new ErrorMessage(channelId, "This is an error message");

            byte[] actual = message.GetBytes();
            String expected =
                "0011000000000000000000000000000000000000000000000000000000000000002a00185468697320697320616e206572726f72206d657373616765";
            
            Assert.Equal(expected, actual.ToHex());
        }
        
        [Fact]
        public void ParseTest()
        {
            ErrorMessage message = new ErrorMessage();
            message.ParsePayload("000000000000000000000000000000000000000000000000000000000000002a00185468697320697320616e206572726f72206d657373616765".HexToByteArray());
            
            byte[] expectedChannelId = new byte[32];
            expectedChannelId[31] = 42;
            
            Assert.Equal(expectedChannelId, message.ChannelId);
            Assert.Equal("This is an error message", message.Data);
        }
    }
}