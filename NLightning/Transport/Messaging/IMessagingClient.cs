using System;

namespace NLightning.Transport.Messaging
{
    public interface IMessagingClient
    {
        MessagingClientState State { get; }
        IObservable<Message> IncomingMessageProvider { get; }
        IObservable<Message> OutgoingMessageProvider { get; }
        IObservable<MessagingClientState> StateProvider{ get; }
        IObservable<MessageValidationException> ValidationExceptionProvider { get; }

        void Connect();
        void Send(Message message);
        void Stop();
    }
}