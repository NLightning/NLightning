using NLightning.Cryptography;
using NLightning.Peer.Channel.Establishment.Messages;
using NLightning.Peer.Channel.Models;
using NLightning.Utils.Extensions;

namespace NLightning.Peer.Channel
{
    public class PendingChannel
    {
        public IPeer Peer { get; }
        public OpenChannelMessage OpenMessage { get; }
        public ECKeyPair RevocationKey { get; }
        public uint ChannelIndex { get; }
        public LocalChannel Channel { get; set; }
        public string TemporaryChannelId { get; }
        
        public PendingChannel(IPeer peer, OpenChannelMessage openMessage, ECKeyPair revocationKey,
            uint channelIndex)
        {
            Peer = peer;
            OpenMessage = openMessage;
            RevocationKey = revocationKey;
            ChannelIndex = channelIndex;
            TemporaryChannelId = openMessage.TemporaryChannelId.ToHex();
        }
    }
}