using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace NLightning.Transport.Messaging
{
    public class MessageWriter
    {
        private readonly NetworkStream _networkStream;
        private readonly TransportState _transportState;
        private readonly object _sendLock = new object();
        private readonly EncryptedMessageBuilder _messageBuilder;
        
        public MessageWriter(TransportState transportState,
            NetworkStream networkStream)
        {
            _networkStream = networkStream;
            _transportState = transportState;
            _messageBuilder = new EncryptedMessageBuilder(transportState);
        }

        public void Write(byte[] data)
        {
            lock (_sendLock)
            {
                _networkStream.Write(data);
            }
        }

        public void Write(Message message)
        {
            if (_transportState.HandshakeState != HandshakeState.Finished)
            {
                throw new InvalidOperationException($"Invalid handshake state {_transportState.HandshakeState}. Must be Finished");
            }
            
            var data = _messageBuilder.Build(message.GetBytes());
            Write(data);
        }
    }
}