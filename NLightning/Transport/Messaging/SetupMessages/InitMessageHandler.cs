using Microsoft.Extensions.Logging;
using NLightning.Peer;
using NLightning.Utils;
using NLightning.Utils.Extensions;

namespace NLightning.Transport.Messaging.SetupMessages
{
    public class InitMessageHandler : MessageHandler<InitMessage>
    {        
        private readonly PeerState _peerState;
        private ILogger _logger;

        public InitMessageHandler(ILoggerFactory loggerFactory, PeerState peerState, NodeAddress remoteNodeAddress)
        {
            _logger = loggerFactory.CreateNodeAddressLogger(GetType(), remoteNodeAddress);
            _peerState = peerState;
        }

        protected override void HandleMessage(InitMessage message)
        {
            PeerFeatures localfeatures = PeerFeatures.Parse(message.Localfeatures);
            
            _logger.LogDebug($"Init Message Received. Remote " + localfeatures);

            _peerState.RemoteFeatures = localfeatures;
        }

        public override void Dispose()
        {
        }
    }
}