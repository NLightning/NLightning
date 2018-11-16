using System.Collections.Generic;
using NLightning.Transport;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.ControlMessages;
using NLightning.Transport.Messaging.SetupMessages;
using NLightning.Transport.Messaging.Validation;
using Xunit;

namespace NLightning.Test.Transport.Messaging
{
    public class MessageParserTests
    {
        [Fact]
        public void ParseTest()
        {
            TransportState transportState = new TransportState();
            byte[] globalFeatures = {0, 5};
            byte[] localFeatures = {0, 6};
            InitMessage referenceMessage = new InitMessage(globalFeatures, localFeatures);
            MessageReader messageReader = new MessageReader(transportState, new List<MessageDefinition> { InitMessage.MessageDefinition }, new List<IMessageValidator>());
            Message message = messageReader.Parse(referenceMessage.GetBytes());
            
            Assert.IsType<InitMessage>(message);
            Assert.Equal(globalFeatures, ((InitMessage)message).Globalfeatures);
            Assert.Equal(localFeatures, ((InitMessage)message).Localfeatures);
        }
        
        [Fact]
        public void ParseNotSupportedMessageTest()
        {
            TransportState transportState = new TransportState();
            PingMessage referenceMessage = new PingMessage(0,0);
            MessageReader messageReader = new MessageReader(transportState, new List<MessageDefinition> { InitMessage.MessageDefinition }, new List<IMessageValidator>());

            Assert.Throws<MessageNotSupportedException>(() => messageReader.Parse(referenceMessage.GetBytes()));
        }
    }
}