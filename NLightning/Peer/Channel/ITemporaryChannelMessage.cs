namespace NLightning.Peer.Channel
{
    public interface ITemporaryChannelMessage
    {
        byte[] TemporaryChannelId { get; }
    }
}