using System.Collections.Generic;
using NLightning.Transport;
using NLightning.Transport.Messaging;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Transport.Messaging
{
    public class EncryptedMessageReaderTests
    {
        [Fact]
        public void ReadTest()
        {
            TransportState transportState = new TransportState();
            transportState.ReceiveDecryptionKey = "969ab31b4d288cedf6218839b27a3e2140827047f2c0f01bf5c04435d43511a9".HexToByteArray();
            EncryptedMessageReader messageReader = new EncryptedMessageReader(transportState);

            byte[] cipherData = "cf2b30ddf0cf3f80e7c35a6e6730b59fe802473180f396d88a8fb0db8cbcf25d2f214cf9ea1d95".HexToByteArray();       
            
            (List<byte[]> actual, int totalRead) = messageReader.Read(cipherData, cipherData.Length);
            Assert.Equal(39, totalRead);
            Assert.Equal("68656c6c6f", actual[0].ToHex());
            
            byte[] cipherData2 = "72887022101f0b6753e0c7de21657d35a4cb2a1f5cde2650528bbc8f837d0f0d7ad833b1a256a1".HexToByteArray();
            (List<byte[]> actual2, int totalRead2) = messageReader.Read(cipherData2, cipherData.Length);
            
            Assert.Equal(39, totalRead2);
            Assert.Equal("68656c6c6f", actual2[0].ToHex());
        }
        
        [Fact]
        public void ReadMultipleTest()
        {
            TransportState transportState = new TransportState();
            transportState.ReceiveDecryptionKey = "969ab31b4d288cedf6218839b27a3e2140827047f2c0f01bf5c04435d43511a9".HexToByteArray();
            EncryptedMessageReader messageReader = new EncryptedMessageReader(transportState);

            byte[] cipherData = "cf2b30ddf0cf3f80e7c35a6e6730b59fe802473180f396d88a8fb0db8cbcf25d2f214cf9ea1d9572887022101f0b6753e0c7de21657d35a4cb504e66cad96173c643306b2ea8a5ff3145ccdfc570".HexToByteArray();
            
            (List<byte[]> actual, int totalRead) = messageReader.Read(cipherData, cipherData.Length);
            
            Assert.Equal(78, totalRead);
            Assert.Equal(2, actual.Count);
            Assert.Equal("68656c6c6f", actual[0].ToHex());
            Assert.Equal("1234567890", actual[1].ToHex());
        }
    }
}