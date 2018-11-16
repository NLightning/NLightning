using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using NLightning.Peer;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;

namespace NLightning.Transport.Messaging
{
    public class MessageReader
    {
        private readonly ICollection<MessageDefinition> _messageDefinitions;
        private readonly IEnumerable<IMessageValidator> _messageValidators;
        private readonly EncryptedMessageReader _encryptedMessageReader;
        
        public MessageReader(TransportState transportState,
            ICollection<MessageDefinition> messageDefinitions, IEnumerable<IMessageValidator> messageValidators)
        {
            _encryptedMessageReader = new EncryptedMessageReader(transportState);
            _messageDefinitions = messageDefinitions;
            _messageValidators = messageValidators;

        }
                
        public async Task<Message> ReadNextMessage(NetworkStream stream)
        {
            var messageLengthData = await stream.ReadExactly(18);
            var messageLength = _encryptedMessageReader.ReadMessageLength(messageLengthData);
            var encryptedMessageData =  await stream.ReadExactly(messageLength + 16);
            var messageData = _encryptedMessageReader.ReadMessageData(encryptedMessageData);
            
            return Parse(messageData);
        }
        
        public Message Parse(byte[] data)
        {
            uint typeId = data.ToUShortBigEndian();

            var definition = _messageDefinitions.FirstOrDefault(def => def.TypeId == typeId);
            if (definition == null)
            {
                throw new MessageNotSupportedException($"Message Type {typeId} not supported");
            }

            try
            {
                Message message = Message.Create(definition);
                message.ParsePayload(data, 2);

                foreach (var messageValidator in _messageValidators)
                {
                    messageValidator.Validate(message, data);
                }

                return message;
            }
            catch (MessageValidationException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new MessageException($"Failed to parse {definition.Type.Name}: {exception}");
            }
        }
    }
}