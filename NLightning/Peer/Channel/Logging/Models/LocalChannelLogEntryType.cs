namespace NLightning.Peer.Channel.Logging.Models
{
    public enum LocalChannelLogEntryType
    {
        StateUpdate,
        Info,
        Warning,
        Error,
        IncomingMessage,
        OutgoingMessage,
        MessageValidationException
    }
}