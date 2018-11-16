using System;
using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Peer.Channel.Models;
using NLightning.Transport;
using NLightning.Transport.Messaging;

namespace NLightning.Peer
{
    public interface IPeerService
    {
        int PeerCount { get; }
        IReadOnlyCollection<IPeer> Peers { get; }
        IObservable<(IPeer Peer, Message Message)> IncomingMessageProvider { get; }
        IObservable<(IPeer Peer, MessagingClientState State)> MessagingStateProvider { get; }
        IObservable<(IPeer Peer, Message Message)> OutgoingMessageProvider { get; }
        IObservable<(IPeer Peer, MessageValidationException ValidationException)> ValidationExceptionProvider { get; }
        IPeer AddPeer(NodeAddress address, bool persist = false, bool reconnect = false);
        void Initialize(ECKeyPair localKey);
    }
}