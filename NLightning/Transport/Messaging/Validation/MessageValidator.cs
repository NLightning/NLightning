namespace NLightning.Transport.Messaging.Validation
{
    public abstract class MessageValidator<T> : IMessageValidator where T : Message
    {
        protected abstract void ValidateMessage(T message, byte[] rawData);

        public void Validate(Message message, byte[] rawData)
        {
            if (message is T variable)
            {
                ValidateMessage(variable, rawData);
            }
        }
    }
}