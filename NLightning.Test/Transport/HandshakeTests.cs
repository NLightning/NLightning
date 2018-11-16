using System;
using NLightning.Cryptography;
using NLightning.Transport;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Transport
{
    public class HandshakeTests
    {
        private String initiatorEphemeralPrivateKey = "1212121212121212121212121212121212121212121212121212121212121212";
        private String responderEphemeralPrivateKey = "2222222222222222222222222222222222222222222222222222222222222222";
        
        private Handshake InitiatorHandshakeMock()
        {
            String remotePublicKey = "028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7";
            String localPrivateKey = "1111111111111111111111111111111111111111111111111111111111111111";
            ECKeyPair localKeyPair = new ECKeyPair(localPrivateKey, true);
            NodeAddress nodeAddress = new NodeAddress(new ECKeyPair(remotePublicKey), "10.0.0.1", 1337);
            return new Handshake(new TransportState(true, localKeyPair, nodeAddress));
        }
        
        private Handshake ResponderHandshakeMock()
        {
            String remotePublicKey = "028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7";
            String localPrivateKey = "2121212121212121212121212121212121212121212121212121212121212121";
            ECKeyPair localKeyPair = new ECKeyPair(localPrivateKey, true);
            NodeAddress nodeAddress = new NodeAddress(new ECKeyPair(remotePublicKey), "10.0.0.1", 1337);
            return new Handshake(new TransportState(false, localKeyPair, nodeAddress));
        }
        
        [Fact]
        public void UninitializedTest()
        {
            var handshake = InitiatorHandshakeMock();
            Assert.Equal(HandshakeState.Uninitialized, handshake.State.HandshakeState);
            Assert.Null(handshake.State.ReceiveDecryptionKey);
            Assert.Null(handshake.State.SendEncryptionKey);
            Assert.Null(handshake.State.ChainingKey);
            Assert.Null(handshake.State.HandshakeHash);
            Assert.Equal(0, handshake.State.SendNonce.Value);
            Assert.Equal(0, handshake.State.ReceiveNonce.Value);
        }

        [Fact]
        public void StateInitializationTest()
        {
            var handshake = InitiatorHandshakeMock();
            handshake.StateInitialization();

            Assert.Null(handshake.State.ReceiveDecryptionKey);
            Assert.Null(handshake.State.SendEncryptionKey);
            Assert.Equal("2640f52eebcd9e882958951c794250eedb28002c05d7dc2ea0f195406042caf1", handshake.State.ChainingKey.ToHex());
            Assert.Equal("8401b3fdcaaa710b5405400536a3d5fd7792fe8e7fe29cd8b687216fe323ecbd", handshake.State.HandshakeHash.ToHex());
            Assert.Equal(0, handshake.State.SendNonce.Value);
            Assert.Equal(0, handshake.State.ReceiveNonce.Value);
            Assert.Equal(HandshakeState.Initialized, handshake.State.HandshakeState);
        }
        
        [Fact]
        public void ApplyActOneTest()
        {
            var handshake = InitiatorHandshakeMock();
            handshake.StateInitialization();
            var payload = handshake.ApplyActOne(new ECKeyPair(initiatorEphemeralPrivateKey, true));

            String expected =
                "00036360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f70df6086551151f58b8afe6c195782c6a";
            
            Assert.Equal(expected, payload.ToHex());
            Assert.Equal(HandshakeState.Act1, handshake.State.HandshakeState);
        }

        [Fact]
        public void ReadActOneRequest()
        {
            var handshake = ResponderHandshakeMock();
            handshake.StateInitialization();

            String input = "00036360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f70df6086551151f58b8afe6c195782c6a";
            String expectedHandshakeHash ="9d1ffbb639e7e20021d9259491dc7b160aab270fb1339ef135053f6f2cebe9ce";
            String expectedChainingKey = "b61ec1191326fa240decc9564369dbb3ae2b34341d1e11ad64ed89f89180582f";
            String expectedTempK1 = "e68f69b7f096d7917245f5e5cf8ae1595febe4d4644333c99f9c4a1282031c9f";
            String expectedResponderEphemeralKey= "036360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f7";
            
            (byte[] tempK1, ECKeyPair responderEpheralKey) = handshake.ReadActOneRequest(input.HexToByteArray(), 50);
            
            Assert.Equal(expectedHandshakeHash, handshake.State.HandshakeHash.ToHex());
            Assert.Equal(expectedChainingKey, handshake.State.ChainingKey.ToHex());
            Assert.Equal(expectedTempK1, tempK1.ToHex());
            Assert.Equal(expectedResponderEphemeralKey, responderEpheralKey.PublicKeyCompressed.ToHex());
            Assert.Equal(HandshakeState.Act1, handshake.State.HandshakeState);
        }
        
        [Fact]
        public void ReadActTwoAnswerTest()
        {
            var handshake = InitiatorHandshakeMock();
            handshake.StateInitialization();
            handshake.ApplyActOne(new ECKeyPair(initiatorEphemeralPrivateKey, true));
            
            String input = "0002466d7fcae563e5cb09a0d1870bb580344804617879a14949cf22285f1bae3f276e2470b93aac583c9ef6eafca3f730ae";
            String expectedHandshakeHash ="90578e247e98674e661013da3c5c1ca6a8c8f48c90b485c0dfa1494e23d56d72";
            String expectedChainingKey = "e89d31033a1b6bf68c07d22e08ea4d7884646c4b60a9528598ccb4ee2c8f56ba";
            String expectedTempK2 = "908b166535c01a935cf1e130a5fe895ab4e6f3ef8855d87e9b7581c4ab663ddc";
            String expectedResponderEpheralKey= "02466d7fcae563e5cb09a0d1870bb580344804617879a14949cf22285f1bae3f27";
            
            (byte[] tempK2, ECKeyPair responderEpheralKey) = handshake.ReadActTwoAnswer(input.HexToByteArray(), 50);
            
            Assert.Equal(expectedHandshakeHash, handshake.State.HandshakeHash.ToHex());
            Assert.Equal(expectedChainingKey, handshake.State.ChainingKey.ToHex());
            Assert.Equal(expectedTempK2, tempK2.ToHex());
            Assert.Equal(expectedResponderEpheralKey, responderEpheralKey.PublicKeyCompressed.ToHex());
            Assert.Equal(HandshakeState.Act2, handshake.State.HandshakeState);
        }
        
        [Fact]
        public void ApplyActTwoTest()
        {
            String readActOneInput = "00036360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f70df6086551151f58b8afe6c195782c6a";
            var handshake = ResponderHandshakeMock();
            handshake.StateInitialization();
            
            (byte[] tempK1, ECKeyPair remoteEphemeralKey) = handshake.ReadActOneRequest(readActOneInput.HexToByteArray(),50);
            var actualResult = handshake.ApplyActTwo(remoteEphemeralKey, new ECKeyPair(responderEphemeralPrivateKey, true));
            
            String expectedResult = "0002466d7fcae563e5cb09a0d1870bb580344804617879a14949cf22285f1bae3f276e2470b93aac583c9ef6eafca3f730ae";
            String expectedHandshakeHash = "90578e247e98674e661013da3c5c1ca6a8c8f48c90b485c0dfa1494e23d56d72";
            String expectedChainingKey = "e89d31033a1b6bf68c07d22e08ea4d7884646c4b60a9528598ccb4ee2c8f56ba";

            Assert.Equal(expectedHandshakeHash, handshake.State.HandshakeHash.ToHex());
            Assert.Equal(expectedChainingKey, handshake.State.ChainingKey.ToHex());
            Assert.Equal(expectedResult, actualResult.ToHex());
            Assert.Equal(HandshakeState.Act2, handshake.State.HandshakeState);
        }
        
        [Fact]
        public void ApplyActThreeTest()
        {
            String actTwoInput = "0002466d7fcae563e5cb09a0d1870bb580344804617879a14949cf22285f1bae3f276e2470b93aac583c9ef6eafca3f730ae";
            var handshake = InitiatorHandshakeMock();
            handshake.StateInitialization();
            handshake.ApplyActOne(new ECKeyPair(initiatorEphemeralPrivateKey, true));
            (byte[] tempK2, ECKeyPair responderEpheralKey) = handshake.ReadActTwoAnswer(actTwoInput.HexToByteArray(),50);

            String expectedResult = "00b9e3a702e93e3a9948c2ed6e5fd7590a6e1c3a0344cfc9d5b57357049aa22355361aa02e55a8fc28fef5bd6d71ad0c38228dc68b1c466263b47fdf31e560e139ba";
            String expectedHandshakeHash = "5dcb5ea9b4ccc755e0e3456af3990641276e1d5dc9afd82f974d90a47c918660";
            String expectedChainingKey = "919219dbb2920afa8db80f9a51787a840bcf111ed8d588caf9ab4be716e42b01";
            String expectedSendKey = "969ab31b4d288cedf6218839b27a3e2140827047f2c0f01bf5c04435d43511a9";
            String expectedReceiveKey = "bb9020b8965f4df047e07f955f3c4b88418984aadc5cdb35096b9ea8fa5c3442";
            
            var actualResult = handshake.ApplyActThree(tempK2, responderEpheralKey);
            
            Assert.Equal(expectedHandshakeHash, handshake.State.HandshakeHash.ToHex());
            Assert.Equal(expectedChainingKey, handshake.State.ChainingKey.ToHex());
            Assert.Equal(expectedSendKey, handshake.State.SendEncryptionKey.ToHex());
            Assert.Equal(expectedReceiveKey, handshake.State.ReceiveDecryptionKey.ToHex());
            Assert.Equal(expectedResult, actualResult.ToHex());
            Assert.Equal(HandshakeState.Finished, handshake.State.HandshakeState);
            Assert.Equal(0, handshake.State.SendNonce.Value);
            Assert.Equal(0, handshake.State.ReceiveNonce.Value);
        }

        [Fact]
        public void ReadActThreeRequestTest()
        {
            String readActOneInput = "00036360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f70df6086551151f58b8afe6c195782c6a";
            String readActThreeInput = "00b9e3a702e93e3a9948c2ed6e5fd7590a6e1c3a0344cfc9d5b57357049aa22355361aa02e55a8fc28fef5bd6d71ad0c38228dc68b1c466263b47fdf31e560e139ba";
            var handshake = ResponderHandshakeMock();
            handshake.StateInitialization();
            (_, ECKeyPair remoteEphemeralKey) = handshake.ReadActOneRequest(readActOneInput.HexToByteArray(),50);
            handshake.ApplyActTwo(remoteEphemeralKey, new ECKeyPair(responderEphemeralPrivateKey, true));
            handshake.ReadActThreeRequest(readActThreeInput.HexToByteArray(), 66);
            
            String expectedReadKey = "969ab31b4d288cedf6218839b27a3e2140827047f2c0f01bf5c04435d43511a9";
            String expectedWriteKey = "bb9020b8965f4df047e07f955f3c4b88418984aadc5cdb35096b9ea8fa5c3442";
            String expectedHandshakeHash = "5dcb5ea9b4ccc755e0e3456af3990641276e1d5dc9afd82f974d90a47c918660";
            String expectedChainingKey = "919219dbb2920afa8db80f9a51787a840bcf111ed8d588caf9ab4be716e42b01";

            Assert.Equal(expectedHandshakeHash, handshake.State.HandshakeHash.ToHex());
            Assert.Equal(expectedChainingKey, handshake.State.ChainingKey.ToHex());
            Assert.Equal(expectedReadKey, handshake.State.ReceiveDecryptionKey.ToHex());
            Assert.Equal(expectedWriteKey, handshake.State.SendEncryptionKey.ToHex());
            Assert.Equal(HandshakeState.Finished, handshake.State.HandshakeState);
            Assert.Equal(0, handshake.State.SendNonce.Value);
            Assert.Equal(0, handshake.State.ReceiveNonce.Value);
        }
    }
}