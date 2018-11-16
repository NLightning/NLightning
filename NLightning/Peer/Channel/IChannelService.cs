using System;
using System.Collections.ObjectModel;
using NLightning.Network;
using NLightning.OnChain;
using NLightning.Peer.Channel.Models;

namespace NLightning.Peer.Channel
{
    public interface IChannelService
    {
        ReadOnlyCollection<PendingChannel> PendingChannels { get; }
        ReadOnlyCollection<LocalChannel> Channels { get; }
        IObservable<LocalChannel> ChannelAddedProvider { get; }
        IObservable<LocalChannel> ChannelRemovedProvider { get; }

        LocalChannel Find(string channelId);
        void Initialize(NetworkParameters networkParameters);
        void AddChannel(IPeer peer, LocalChannel channel);
        void UpdateChannel(LocalChannel channel);
        void RemoveChannel(LocalChannel channel);
        void AddPendingChannel(PendingChannel pendingChannel);
        void RemovePendingChannel(PendingChannel pendingChannel);
        uint GetNextChannelIndex();
    }
}