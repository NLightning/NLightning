using System;

namespace NLightning.Transport.Messaging
{
    public class MessageException : Exception
    {
        public MessageException(String message) : base(message)
        {
            
        }
    }
}