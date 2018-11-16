namespace NLightning.Peer.Channel.Logging.Models
{
    public enum LocalChannelError
    {
        InvalidState,
        ValidationError,
        InvalidShutdownScriptPubKey,
        InvalidCommitmentSignature
    }
}