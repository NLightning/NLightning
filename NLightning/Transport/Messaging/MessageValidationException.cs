namespace NLightning.Transport.Messaging
{
    public class MessageValidationException : MessageException
    {
        public byte[] FailChannelId { get; }
        public Message MessageToValidate { get; }
        public bool FailConnection { get; }
        
        public MessageValidationException(Message messageToValidate, string message, bool failConnection = false, 
                                            byte[] failChannelId = null) : base(message)
        {
            MessageToValidate = messageToValidate;
            FailConnection = failConnection;
            FailChannelId = failChannelId;
        }

    }
}