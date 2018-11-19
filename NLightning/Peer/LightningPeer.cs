using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NLightning.Cryptography;
using NLightning.Transport;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;

namespace NLightning.Peer
{
    public class LightningPeer : IPeer
    {
        private readonly ILogger _logger;
        private readonly ECKeyPair _localKeyPair;
        private readonly NodeAddress _remoteNodeAddress;
        private readonly MessagingClient _messagingClient;
        private readonly PeerState _peerState;

        public LightningPeer(ILoggerFactory loggerFactory, IEnumerable<IMessageValidator> messageValidators,
            ECKeyPair localKeyPair, NodeAddress remoteNodeAddress)
        {
            _logger = loggerFactory.CreateNodeAddressLogger(GetType(), remoteNodeAddress);
            _localKeyPair = localKeyPair;
            _remoteNodeAddress = remoteNodeAddress;
            _peerState = new PeerState(new PeerFeatures(false, false, false, false, false, false, true));
            _messagingClient = new MessagingClient(loggerFactory, messageValidators, _peerState, _localKeyPair, remoteNodeAddress);
        }

        public PeerState State => _peerState;
        public IMessagingClient Messaging => _messagingClient;
        public NodeAddress NodeAddress => _remoteNodeAddress;

        public string PublicKey => NodeAddress.PublicKey.PublicKeyCompressed.ToHex();
        public bool Reconnect { get; set; }

        public void Connect()
        {
            _messagingClient.Connect();
        }

        public void Disconnect()
        {
            Reconnect = false;
            _messagingClient.Stop();
        }
    }
}