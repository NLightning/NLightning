namespace NLightning.Peer.Channel.Models
{
    public enum LocalChannelState
    {
        Uninitialized = 0,
        OpenChannel = 1,
        AcceptChannel = 2,
        FundingCreated = 3,
        FundingSigned = 4,
        FundingLocked = 5,
        FundingFailed = 6,
        NormalOperation = 7,
        Shutdown = 8,
        ClosingSigned = 9,
        LocalUnilateralClose = 10,
        RemoteUnilateralClose = 11,
        Penalty = 12,
        Closed = 13
    }
}