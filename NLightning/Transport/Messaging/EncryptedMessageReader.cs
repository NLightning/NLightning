using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Utils;
using NLightning.Utils.Extensions;

namespace NLightning.Transport.Messaging
{
    public class EncryptedMessageReader
    {
        private readonly TransportState _transportState;

        public EncryptedMessageReader(TransportState transportState)
        {
            _transportState = transportState;
        }

        public (List<byte[]> messages, int totalRead) Read(byte[] messageData, int length)
        {
            int position = 0;
            List<byte[]> messages = new List<byte[]>();

            if (length < 18)
            {
                return (messages, 0);
            }
            
            while (position < length)
            {
                (byte[] decryptedLength, _) = ChaCha20Poly1305.DecryptWithAdditionalData(_transportState.ReceiveDecryptionKey, 
                    _transportState.ReceiveNonce.GetBytes(), new byte[0], messageData.SubArray(position, 18));
            
                ushort messageLength = decryptedLength.ToUShortBigEndian();
                if (messageLength == 0)
                {
                    throw new MessageException("Invalid message length");
                }
                
                int encryptedMessageLength = 18 + messageLength + 16;
                int currentMessageLength = length - position;
                if (encryptedMessageLength > currentMessageLength)
                {
                    return (messages, position);
                }
                
                _transportState.ReceiveNonce.Increment();
                (byte[] decryptedMessage, _) = ChaCha20Poly1305.DecryptWithAdditionalData(_transportState.ReceiveDecryptionKey, 
                    _transportState.ReceiveNonce.GetBytes(), new byte[0], messageData.SubArray(position + 18, messageLength));
            
                _transportState.ReceiveNonce.Increment();
                messages.Add(decryptedMessage);

                position = position + encryptedMessageLength;
            }
            
            return (messages, position);
        }

        public int ReadMessageLength(byte[] messageData)
        {
            (byte[] decryptedLength, _) = ChaCha20Poly1305.DecryptWithAdditionalData(_transportState.ReceiveDecryptionKey, 
                _transportState.ReceiveNonce.GetBytes(), new byte[0], messageData);
            
            _transportState.ReceiveNonce.Increment();
            
            int messageLength = decryptedLength.ToUShortBigEndian();
            int messageMax = 65535;
            if (messageLength - 16 > messageMax)
            {
                throw new MessageException($"Invalid message length {messageLength}. Maximum: {messageMax}");
            }

            return messageLength;
        }

        public byte[] ReadMessageData(byte[] messageData)
        {
            (byte[] decryptedMessage, _) = ChaCha20Poly1305.DecryptWithAdditionalData(_transportState.ReceiveDecryptionKey, 
                _transportState.ReceiveNonce.GetBytes(), new byte[0], messageData);
            
            _transportState.ReceiveNonce.Increment();

            if (_transportState.ReceiveNonce.Value == 1000)
            {
                _transportState.RotateReceiveKey();
            }
            
            return decryptedMessage.SubArray(0, messageData.Length - 16);
        }
    }
}