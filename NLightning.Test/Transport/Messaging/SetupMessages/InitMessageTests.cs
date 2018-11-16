using NLightning.Transport.Messaging.SetupMessages;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Transport.Messaging.SetupMessages
{
    public class InitMessageTests
    {
        [Fact]
        public void GetBytesTest()
        {
            byte[] globalFeatures = {0, 5};
            byte[] localFeatures = {0, 6};
            InitMessage message = new InitMessage(globalFeatures, localFeatures);

            byte[] actual = message.GetBytes();
            
            Assert.Equal("00100002000500020006", actual.ToHex());
        }
        
        [Fact]
        public void ParseTest()
        {
            InitMessage message = new InitMessage();
            message.ParsePayload("0002000500020006".HexToByteArray());
            
            byte[] expectedGlobalFeatures = {0, 5};
            byte[] expectedLocalFeatures = {0, 6};
            
            Assert.Equal(expectedLocalFeatures, message.Localfeatures);
            Assert.Equal(expectedGlobalFeatures, message.Globalfeatures);
        }
    }
}