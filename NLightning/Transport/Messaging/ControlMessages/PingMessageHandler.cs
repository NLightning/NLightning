namespace NLightning.Transport.Messaging.ControlMessages
{
    public class PingMessageHandler : MessageHandler<PingMessage>
    {
        private readonly MessageWriter _messageWriter;
        
        public PingMessageHandler(MessageWriter messageWriter)
        {
            _messageWriter = messageWriter;
        }

        protected override void HandleMessage(PingMessage message)
        {
            _messageWriter.Write(new PongMessage(message.PongDataLength));
        }

        public override void Dispose()
        {
        }
    }
}