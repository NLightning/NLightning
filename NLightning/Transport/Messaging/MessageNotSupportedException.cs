using System;

namespace NLightning.Transport.Messaging
{
    public class MessageNotSupportedException : Exception
    {
        public MessageNotSupportedException(String message) : base(message)
        {
            
        }
    }
}