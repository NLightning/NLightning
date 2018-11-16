using System;
using Microsoft.Extensions.Logging;
using NLightning.Peer;
using NLightning.Utils;
using NLightning.Utils.Extensions;

namespace NLightning.Transport.Messaging.ControlMessages
{
    public class PongMessageHandler : MessageHandler<PongMessage>
    {
        private readonly MessageWriter _messageWriter;
        private readonly PeerState _peerState;
        private readonly System.Timers.Timer _timer = new System.Timers.Timer(60000);
        private DateTime _lastPingTimestamp;
        private bool _pingActive;
        private ILogger _logger;

        public PongMessageHandler(ILoggerFactory loggerFactory, MessageWriter messageWriter, PeerState peerState,
            NodeAddress remoteNodeAddress)
        {
            _logger = loggerFactory.CreateNodeAddressLogger(GetType(), remoteNodeAddress);
            _messageWriter = messageWriter;
            _peerState = peerState;
            _timer.Enabled = true;
            _timer.Elapsed += (sender, args) => Ping();
        }

        private void Ping()
        {
            if (_peerState.TransportState.HandshakeState != HandshakeState.Finished)
            {
                return;
            }
            
            _pingActive = true;
            _messageWriter.Write(new PingMessage(0,0));
            _lastPingTimestamp = DateTime.Now;
        }
        
        protected override void HandleMessage(PongMessage message)
        {
            if (!_pingActive)
            {
                return;
            }

            var latency = DateTime.Now - _lastPingTimestamp;
            _logger.LogDebug($"Ping Latency: {latency.Milliseconds} ms");
            _peerState.Latency = latency;
            _pingActive = false;
        }

        public override void Dispose()
        {
            _timer.Stop();
        }
    }
}