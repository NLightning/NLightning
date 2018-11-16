using NLightning.Transport;
using NLightning.Transport.Messaging;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Transport.Messaging
{
    public class EncryptedMessageBuilderTests
    {
        [Fact]
        public void BuildTest()
        {
            TransportState transportState = new TransportState();
            transportState.SendEncryptionKey = "969ab31b4d288cedf6218839b27a3e2140827047f2c0f01bf5c04435d43511a9".HexToByteArray();

            EncryptedMessageBuilder encryptedMessageBuilder = new EncryptedMessageBuilder(transportState);
            byte[] result1 = encryptedMessageBuilder.Build("68656c6c6f".HexToByteArray());
            
            Assert.Equal("cf2b30ddf0cf3f80e7c35a6e6730b59fe802473180f396d88a8fb0db8cbcf25d2f214cf9ea1d95", result1.ToHex());
            
            byte[] result2 = encryptedMessageBuilder.Build("1234567890".HexToByteArray());
            Assert.Equal("72887022101f0b6753e0c7de21657d35a4cb504e66cad96173c643306b2ea8a5ff3145ccdfc570", result2.ToHex());
        }

        [Fact]
        public void BuildTestWithKeyRotation()
        {
            TransportState transportState = new TransportState();
            EncryptedMessageBuilder encryptedMessageBuilder = new EncryptedMessageBuilder(transportState);
            transportState.SendEncryptionKey = "969ab31b4d288cedf6218839b27a3e2140827047f2c0f01bf5c04435d43511a9".HexToByteArray();
            transportState.ChainingKey = "919219dbb2920afa8db80f9a51787a840bcf111ed8d588caf9ab4be716e42b01".HexToByteArray();
            
            for (int i = 0; i < 500; i++)
            {
                encryptedMessageBuilder.Build("68656c6c6f".HexToByteArray());
            }
           
            byte[] result500 = encryptedMessageBuilder.Build("68656c6c6f".HexToByteArray());
            Assert.Equal("178cb9d7387190fa34db9c2d50027d21793c9bc2d40b1e14dcf30ebeeeb220f48364f7a4c68bf8", result500.ToHex());
            
            byte[] result501 = encryptedMessageBuilder.Build("68656c6c6f".HexToByteArray());
            Assert.Equal("1b186c57d44eb6de4c057c49940d79bb838a145cb528d6e8fd26dbe50a60ca2c104b56b60e45bd", result501.ToHex());
            
            for (int i = 501; i < 999; i++)
            {
                encryptedMessageBuilder.Build("68656c6c6f".HexToByteArray());
            }
            
            byte[] result1000 = encryptedMessageBuilder.Build("68656c6c6f".HexToByteArray());
            Assert.Equal("4a2f3cc3b5e78ddb83dcb426d9863d9d9a723b0337c89dd0b005d89f8d3c05c52b76b29b740f09", result1000.ToHex());
            
            byte[] result1001 = encryptedMessageBuilder.Build("68656c6c6f".HexToByteArray());
            Assert.Equal("2ecd8c8a5629d0d02ab457a0fdd0f7b90a192cd46be5ecb6ca570bfc5e268338b1a16cf4ef2d36", result1001.ToHex());
        }
    }
}