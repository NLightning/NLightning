using System;
using NLightning.Peer.Channel.Models;

namespace NLightning.Peer.Channel.Establishment
{
    public interface IChannelReestablishmentService
    {
        IObservable<(IPeer Peer, LocalChannel Channel)> ChannelReestablishedProvider { get; }
        void Initialize();
    }
}