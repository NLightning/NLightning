namespace NLightning.Peer.Channel
{
    public interface IChannelMessage
    {
        byte[] ChannelId { get; }
    }
}