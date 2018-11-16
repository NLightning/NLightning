using System;
using NLightning.Peer.Channel.Models;

namespace NLightning.Peer.Channel
{
    public interface IChannelStateService
    {
        IObservable<LocalChannel> ChannelActiveStateChangedProvider { get; }
        void Initialize();
    }
}