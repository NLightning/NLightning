namespace NLightning.Peer.Channel.Models
{
    public enum CloseReason
    {
        None,
        LocalMutualClose,
        RemoteMutualClose,
        LocalUnilateralClose,
        RemoteUnilateralClose,
        ChannelFailure,
        InvalidShutdownPubKey,
        Penalty
    }
}