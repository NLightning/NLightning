namespace NLightning.Transport.Messaging.Validation
{
    public interface IMessageValidator
    {
        void Validate(Message message, byte[] rawData);
    }
}