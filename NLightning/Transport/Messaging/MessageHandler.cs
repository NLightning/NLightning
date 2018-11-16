namespace NLightning.Transport.Messaging
{
    public abstract class MessageHandler<T> : IMessageHandler where T : Message
    {
        protected abstract void HandleMessage(T message);

        public void Handle(Message message)
        {
            if (message is T variable)
            {
                HandleMessage(variable);
            }
        }

        public abstract void Dispose();
    }
}