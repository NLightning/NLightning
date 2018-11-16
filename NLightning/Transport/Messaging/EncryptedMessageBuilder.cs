using NLightning.Cryptography;
using NLightning.Utils;
using NLightning.Utils.Extensions;

namespace NLightning.Transport.Messaging
{
    public class EncryptedMessageBuilder
    {
        private readonly TransportState _transportState;

        public EncryptedMessageBuilder(TransportState transportState)
        {
            _transportState = transportState;
        }

        public byte[] Build(byte[] messageData)
        {
            if (messageData.Length > 65535)
            {
                throw new MessageException("Message size exceeds maximum size.");
            }
            
            (byte[] encryptedLength, byte[] lengthMac) = ChaCha20Poly1305.EncryptWithAdditionalData(_transportState.SendEncryptionKey, 
                _transportState.SendNonce.GetBytes(), new byte[0], ((ushort)messageData.Length).GetBytesBigEndian());
            
            _transportState.SendNonce.Increment();
            
            (byte[] encryptedMessage, byte[] messageMac) = ChaCha20Poly1305.EncryptWithAdditionalData(_transportState.SendEncryptionKey, 
                _transportState.SendNonce.GetBytes(), new byte[0], messageData);
            
            _transportState.SendNonce.Increment();
            
            if (_transportState.SendNonce.Value == 1000)
            {
                _transportState.RotateSendKey();
            }
            
            return ByteExtensions.Combine(encryptedLength, lengthMac, encryptedMessage, messageMac);
        }
    }
}