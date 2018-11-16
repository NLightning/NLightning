using System;
using NLightning.Network;
using NLightning.Peer.Channel.Models;

namespace NLightning.Peer.Channel
{
    public interface IChannelEstablishmentService
    {
        IObservable<(IPeer Peer, PendingChannel PendingChannel, string Error)> FailureProvider { get; }
        IObservable<(IPeer Peer, LocalChannel Channel)> SuccessProvider { get; }
        
        PendingChannel OpenChannel(IPeer peer, ulong fundingSatoshis, ulong pushMSat = 0);
        void Initialize(NetworkParameters networkParameters);
    }
}