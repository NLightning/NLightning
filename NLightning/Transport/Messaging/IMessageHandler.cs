using System;

namespace NLightning.Transport.Messaging
{
    internal interface IMessageHandler : IDisposable
    {
        void Handle(Message message);
    }
}