using System;
using NLightning.Network;
using NLightning.Peer.Channel.Models;

namespace NLightning.Peer.Channel
{
    public interface IChannelCloseService
    {
        IObservable<LocalChannel> ChannelClosingProvider { get; }
        IObservable<LocalChannel> ChannelClosedProvider { get; }
        
        void Close(LocalChannel channel, bool unilateralCloseOnUnavailability = true);
        void UnilateralClose(LocalChannel channel);
        void Initialize(NetworkParameters networkParameters);
        void ShutdownUnknownChannel(IPeer peer, byte[] channelId);
    }
}