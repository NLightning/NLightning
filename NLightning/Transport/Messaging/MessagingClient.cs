using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLightning.Cryptography;
using NLightning.Peer;
using NLightning.Transport.Messaging.ControlMessages;
using NLightning.Transport.Messaging.SetupMessages;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;

namespace NLightning.Transport.Messaging
{
    public class MessagingClient : IMessagingClient, IDisposable
    {
        private readonly Subject<MessagingClientState> _stateProvider = new Subject<MessagingClientState>();
        private readonly ILogger _logger;
        private readonly PeerState _peerState;
        private readonly ECKeyPair _localKeyPair;
        private readonly NodeAddress _remoteNodeAddress;
        private readonly Subject<Message> _incomingMessageProvider = new Subject<Message>();
        private readonly Subject<Message> _outgoingMessageProvider = new Subject<Message>();
        private readonly Subject<MessageValidationException> _validationExceotionProvider = new Subject<MessageValidationException>();
        private readonly IEnumerable<IMessageValidator> _messageValidators;
        private TcpClient _client;
        private List<IMessageHandler> _messageHandlers;
        private MessageReader _messageReader;
        private MessageWriter _messageWriter;
        private Handshake _handshake;
        private MessagingClientState _state = MessagingClientState.Uninitialized;

        private ILoggerFactory _loggerFactory;

        public MessagingClient()
        {
        }
        
        public MessagingClient(ILoggerFactory loggerFactory, IEnumerable<IMessageValidator> messageValidators,
            PeerState peerState, ECKeyPair localKeyPair, NodeAddress remoteNodeAddress)
        {
            _logger = loggerFactory.CreateNodeAddressLogger(GetType(), remoteNodeAddress);
            _loggerFactory = loggerFactory;
            _messageValidators = messageValidators;
            _peerState = peerState;
            _localKeyPair = localKeyPair;
            _remoteNodeAddress = remoteNodeAddress;
        }

        public MessagingClientState State
        {
            get => _state;
            private set
            {
                _state = value;
                _stateProvider.OnNext(value);
            }
        }

        public IObservable<Message> IncomingMessageProvider => _incomingMessageProvider;
        public IObservable<Message> OutgoingMessageProvider => _outgoingMessageProvider;
        public IObservable<MessagingClientState> StateProvider => _stateProvider;
        public IObservable<MessageValidationException> ValidationExceptionProvider => _validationExceotionProvider;
        
        public void Connect()
        {
            try
            {
                if (_client != null)
                {
                    throw new InvalidOperationException("Client can't be reused");
                }

                _client = new TcpClient();
                _client.Connect(_remoteNodeAddress.IpAddress, _remoteNodeAddress.Port);
                _logger.LogInformation($"Connected to: {_remoteNodeAddress.IpAddress}:{_remoteNodeAddress.Port}");

                _peerState.TransportState = new TransportState(true, _localKeyPair, _remoteNodeAddress);
            
                InitializeReaderWriterAndHandler();
                StartHandshake();
            }
            catch (Exception)
            {
                Stop();
                throw;
            }
        }

        private void StartHandshake()
        {
            if (_handshake != null)
            {
                throw new InvalidOperationException("Handshake can only be done once.");
            }
         
            State = MessagingClientState.Handshake;
            _handshake = new Handshake(_peerState.TransportState);
            _handshake.StateInitialization();
            
            Task.Run(() => Read(_client));
            _messageWriter.Write(_handshake.ApplyActOne());
        }
        
        private void InitializeReaderWriterAndHandler()
        {
            _messageReader = new MessageReader(_peerState.TransportState, MessageDefinitions.Definitions, _messageValidators);
            _messageWriter = new MessageWriter(_peerState.TransportState, _client.GetStream());
            _messageHandlers = new List<IMessageHandler>
            { 
                new PingMessageHandler(_messageWriter), 
                new PongMessageHandler(_loggerFactory, _messageWriter, _peerState, _remoteNodeAddress),
                new InitMessageHandler(_loggerFactory, _peerState, _remoteNodeAddress),
                new ErrorMessageHandler(_loggerFactory, _remoteNodeAddress)
            };
        }
        
        private async Task Read(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                if (client.Connected && _peerState.TransportState.HandshakeState == HandshakeState.Initialized)
                {
                    await _handshake.ReadHandshake(stream);
                }

                if (client.Connected && _peerState.TransportState.HandshakeState == HandshakeState.Finished)
                {
                    _messageWriter.Write(new InitMessage(new byte[] {0}, _peerState.PeerFeatures.GetBytes()));
                    State = MessagingClientState.Active;
                    await ReadMessages(stream);
                }
            }
            catch (Exception exception)
            {
                if (exception is IOException || exception is ObjectDisposedException)
                {
                    _logger.LogDebug($"Read ended ({exception.Message}). Stopping ...");
                }
                else
                {
                    _logger.LogError($"Failed to read messages: {exception}", exception);
                }
            }
            
            Stop();
        }

        private async Task ReadMessages(NetworkStream stream)
        {
            while (true)
            {
                try
                {
                    HandleMessage(await _messageReader.ReadNextMessage(stream));
                }
                catch (MessageValidationException exception)
                {
                    _validationExceotionProvider.OnNext(exception);
                    if (exception.FailChannelId != null)
                    {
                        _logger.LogError("Failed to validate message. " +
                                         $"will fail Channel ID: {exception.FailChannelId.ToHex()}. " +
                                         $"Error: {exception}", exception);
                        
                        Send(new ErrorMessage(exception.FailChannelId, exception.Message));
                    }
                    
                    if (exception.FailConnection)
                    {
                        _logger.LogError("Failed to validate message. " +
                                         $"Will fail connection: {exception.FailConnection}" +
                                         $"Error: {exception}", exception);
                        throw;
                    }
                    
                    _logger.LogDebug($"Failed to validate message. Will continue reading. Error: {exception.Message}");
                }
                catch (MessageNotSupportedException notSupportedException)
                {
                    _logger.LogWarning(notSupportedException.Message);
                }
            }
        }

        private void HandleMessage(Message message)
        {
            _logger.LogDebug($"Received message: {message.Definition.Type.Name}");
            foreach (var messageHandler in _messageHandlers)
            {
                try
                {
                    messageHandler.Handle(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{messageHandler.GetType()} failed to handle message: {ex}", ex);
                }
            }
            
            _incomingMessageProvider.OnNext(message);
        }

        public void Send(Message message)
        {
            try
            {
                _outgoingMessageProvider.OnNext(message);
                _messageWriter.Write(message);
                _logger.LogDebug($"Sent message: {message.GetType()}");
            }
            catch (ObjectDisposedException)
            {
                _logger.LogWarning("Send failed: accessing a disposed stream. Stopping client.");
                Stop();
            }
            catch (SocketException exception)
            {
                _logger.LogWarning($"Send failed: socket exception: {exception.Message}. Stopping client.");
                Stop();
            }
            catch (IOException exception)
            {
                _logger.LogWarning($"Send failed: IO exception: {exception.Message}. Stopping client.");
                Stop();
            }
        }

        public void Stop()
        {
            if (State == MessagingClientState.Stopped)
            {
                return;
            }

            State = MessagingClientState.Stopped;
            
            Dispose();

            _logger.LogDebug("Stopped Messaging Client");
        }

        public void Dispose()
        {
            _stateProvider.Dispose();
            _incomingMessageProvider.Dispose();
            _outgoingMessageProvider.Dispose();
            _validationExceotionProvider.Dispose();
            _messageHandlers?.ForEach(handler => handler.Dispose());
            _client?.Close();
        }
    }
}