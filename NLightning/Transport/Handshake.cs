using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NLightning.Cryptography;
using NLightning.Utils;
using NLightning.Utils.Extensions;

namespace NLightning.Transport
{
    public class Handshake
    {
        private static readonly byte[] ProtocolName = Encoding.ASCII.GetBytes("Noise_XK_secp256k1_ChaChaPoly_SHA256");
        private static readonly byte[] Prologue = Encoding.ASCII.GetBytes("lightning");
        private readonly TransportState _state;
        
        public Handshake(TransportState transportState)
        {
            _state = transportState;
        }

        public TransportState State => _state;

        // https://github.com/lightningnetwork/lightning-rfc/blob/master/08-transport.md#handshake-state-initialization
        public void StateInitialization()
        {
            byte[] publicKey = _state.IsInitiatingNode
                ? _state.RemoteAddress.PublicKey.PublicKeyCompressed
                : _state.LocalKeyPair.PublicKeyCompressed;
            byte[] handshakeHash = SHA256.ComputeHash(ProtocolName);
            
            _state.ChainingKey = handshakeHash;
            handshakeHash = SHA256.ComputeHash(handshakeHash.ConcatToNewArray(Prologue));
            handshakeHash = SHA256.ComputeHash(handshakeHash.ConcatToNewArray(publicKey));
            _state.HandshakeHash = handshakeHash;
            _state.HandshakeState = HandshakeState.Initialized;
        }
    
        public byte[] ApplyActOne(ECKeyPair ephemeralKeyPair = null)
        {
            if (_state.HandshakeState != HandshakeState.Initialized)
            {
                throw new InvalidOperationException($"Invalid handshake state {_state.HandshakeState}. Must be Initialized");
            }
            
            _state.EphemeralKeyPair = ephemeralKeyPair ?? Secp256K1.GenerateKeyPair();
            byte[] handshakeHash = SHA256.ComputeHash(_state.HandshakeHash.ConcatToNewArray(_state.EphemeralKeyPair.PublicKeyCompressed));
            byte[] ss = ECDH.ComputeHashedPoint(_state.RemoteAddress.PublicKey.PublicKeyParameters, _state.EphemeralKeyPair.PrivateKey);
            (byte[] chainingKey, byte[] tempK1) = HmacSha256.ComputeHashes(_state.ChainingKey, ss);
            _state.ChainingKey = chainingKey;
            (_, byte[] tag) = ChaCha20Poly1305.EncryptWithAdditionalData(tempK1, _state.SendNonce.GetBytes(), handshakeHash, new byte[0]);
            handshakeHash = SHA256.ComputeHash(handshakeHash.ConcatToNewArray(tag));
            
            _state.HandshakeHash = handshakeHash;
            _state.HandshakeState = HandshakeState.Act1;
            return ByteExtensions.Combine(new byte[] {0}, _state.EphemeralKeyPair.PublicKeyCompressed, tag);
        }

        public (byte[] tempK1, ECKeyPair ephemeralKey) ReadActOneRequest(Span<byte> message, int length)
        {
            if (_state.HandshakeState != HandshakeState.Initialized)
            {
                throw new InvalidOperationException($"Invalid handshake state {_state.HandshakeState}. Must be Initialized");
            }
            
            if (length != 50)
            {
                throw new ArgumentException("ACT1_READ_FAILED");
            }
            
            byte version = message.Slice(0, 1)[0];
            ECKeyPair responderEphemeralKey = new ECKeyPair(message.Slice(1, 33), false);
            Span<byte> c = message.Slice(34, 16);

            if (version != 0)
            {
                throw new NotSupportedException("ACT1_BAD_VERSION");
            }
            
            byte[] handshakeHash = SHA256.ComputeHash(_state.HandshakeHash.ConcatToNewArray(responderEphemeralKey.PublicKeyCompressed));
            byte[] ss = ECDH.ComputeHashedPoint(responderEphemeralKey.PublicKeyParameters, _state.LocalKeyPair.PrivateKey);
            (byte[] chainingKey, byte[] tempK1) = HmacSha256.ComputeHashes(_state.ChainingKey, ss);
            _state.ChainingKey = chainingKey;
            
            (_, byte[] mac) = ChaCha20Poly1305.DecryptWithAdditionalData(tempK1, _state.ReceiveNonce.GetBytes(), handshakeHash, new byte[0]);

            if (!c.SequenceEqual(mac))
            {
                throw new ArgumentException("ACT1_BAD_TAG");
            }
            
            _state.ReceiveNonce.Increment();
            _state.HandshakeHash = SHA256.ComputeHash(handshakeHash.ConcatToNewArray(c.ToArray()));
            _state.HandshakeState = HandshakeState.Act1;
            return (tempK1, responderEphemeralKey);
        }
        
        public byte[] ApplyActTwo(ECKeyPair initiatorEphemeralKey, ECKeyPair localEphemeralKey = null)
        {
            if (_state.HandshakeState != HandshakeState.Act1)
            {
                throw new InvalidOperationException($"Invalid Handshake state {_state.HandshakeState}. Must be Act1");
            }
            
            _state.EphemeralKeyPair = localEphemeralKey ?? Secp256K1.GenerateKeyPair();
            byte[] handshakeHash = SHA256.ComputeHash(_state.HandshakeHash.ConcatToNewArray(_state.EphemeralKeyPair.PublicKeyCompressed));
            byte[] ss = ECDH.ComputeHashedPoint(initiatorEphemeralKey.PublicKeyParameters, _state.EphemeralKeyPair.PrivateKey);
            (byte[] chainingKey, byte[] tempK2) = HmacSha256.ComputeHashes(_state.ChainingKey, ss);
            (_, byte[] tag1) = ChaCha20Poly1305.EncryptWithAdditionalData(tempK2, new byte[12], handshakeHash, new byte[0]);
            handshakeHash = SHA256.ComputeHash(handshakeHash.ConcatToNewArray(tag1));
            
            _state.HandshakeHash = handshakeHash;
            _state.ChainingKey = chainingKey;
            _state.TempKey2 = tempK2;
            _state.HandshakeState = HandshakeState.Act2;
            return ByteExtensions.Combine(new byte[] {0}, _state.EphemeralKeyPair.PublicKeyCompressed, tag1);
        }
        
        public (byte[] tempK2, ECKeyPair ephemeralKey) ReadActTwoAnswer(Span<byte> message, int length)
        {
            if (_state.HandshakeState != HandshakeState.Act1)
            {
                throw new InvalidOperationException($"Invalid Handshake state {_state.HandshakeState}. Must be Act1");
            }
            
            if (length != 50)
            {
                throw new ArgumentException("ACT2_READ_FAILED");
            }
            
            byte version = message.Slice(0, 1)[0];
            ECKeyPair responderEphemeralKey = new ECKeyPair(message.Slice(1, 33), false);
            Span<byte> c = message.Slice(34, 16);

            if (version != 0)
            {
                throw new NotSupportedException("ACT2_BAD_VERSION");
            }
            
            byte[] handshakeHash = SHA256.ComputeHash(_state.HandshakeHash.ConcatToNewArray(responderEphemeralKey.PublicKeyCompressed));
            byte[] ss = ECDH.ComputeHashedPoint(responderEphemeralKey.PublicKeyParameters, _state.EphemeralKeyPair.PrivateKey);
            (byte[] chainingKey, byte[] tempK2) = HmacSha256.ComputeHashes(_state.ChainingKey, ss);
            _state.ChainingKey = chainingKey;
            
            (_, byte[] mac) = ChaCha20Poly1305.DecryptWithAdditionalData(tempK2, _state.SendNonce.GetBytes(), handshakeHash, new byte[0]);

            if (!c.SequenceEqual(mac))
            {
                throw new ArgumentException("ACT2_BAD_TAG");
            }
            
            _state.SendNonce.Increment();
            _state.HandshakeHash = SHA256.ComputeHash(handshakeHash.ConcatToNewArray(c.ToArray()));
            _state.HandshakeState = HandshakeState.Act2;
            return (tempK2, responderEphemeralKey);
        }
        
        public byte[] ApplyActThree(byte[] tempK2, ECKeyPair responderEphemeralKey)
        {
            if (_state.HandshakeState != HandshakeState.Act2)
            {
                throw new InvalidOperationException($"Invalid Handshake state {_state.HandshakeState}. Must be Act2");
            }
            
            (byte[] cipherText, byte[] tag) = ChaCha20Poly1305.EncryptWithAdditionalData(tempK2, _state.SendNonce.GetBytes(), _state.HandshakeHash, _state.LocalKeyPair.PublicKeyCompressed);
            byte[] cipherAndMac = cipherText.ConcatToNewArray(tag);
            byte[] handshakeHash = SHA256.ComputeHash(_state.HandshakeHash.ConcatToNewArray(cipherAndMac));
            byte[] ss = ECDH.ComputeHashedPoint(responderEphemeralKey.PublicKeyParameters, _state.LocalKeyPair.PrivateKey);
            (byte[] chainingKey, byte[] tempK3) = HmacSha256.ComputeHashes(_state.ChainingKey, ss);
            (_, byte[] tag2) = ChaCha20Poly1305.EncryptWithAdditionalData(tempK3, new byte[12], handshakeHash, new byte[0]);
            (byte[] encryptKey, byte[] decryptKey) = HmacSha256.ComputeHashes(chainingKey, new byte[0]);

            _state.HandshakeHash = handshakeHash;
            _state.ChainingKey = chainingKey;
            _state.SendEncryptionKey = encryptKey;
            _state.ReceiveDecryptionKey = decryptKey;
            _state.SendNonce.Reset();
            _state.ReceiveNonce.Reset();
            _state.HandshakeState = HandshakeState.Finished;
            return ByteExtensions.Combine(new byte[] {0}, cipherAndMac, tag2);
        }
        
        public void ReadActThreeRequest(Span<byte> message, int length)
        {
            if (length != 66)
            {
                throw new ArgumentException("ACT3_READ_FAILED");
            }
            
            byte version = message.Slice(0, 1)[0];
            Span<byte> c = message.Slice(1, 49);
            //byte[] t = message.SubArray(50, 16);

            if (version != 0)
            {
                throw new NotSupportedException("ACT3_BAD_VERSION");
            }

            (byte[] rs, _) = ChaCha20Poly1305.DecryptWithAdditionalData(_state.TempKey2, _state.ReceiveNonce.GetBytes(), _state.HandshakeHash, c.ToArray());
            ECKeyPair rsKey = new ECKeyPair(((Span<byte>)rs).Slice(0,33), false);
            byte[] handshakeHash = SHA256.ComputeHash(_state.HandshakeHash.ConcatToNewArray(c.ToArray()));
            byte[] ss = ECDH.ComputeHashedPoint(rsKey.PublicKeyParameters, _state.EphemeralKeyPair.PrivateKey);
            (byte[] chainingKey, _) = HmacSha256.ComputeHashes(_state.ChainingKey, ss);
            
            //byte[] p, byte[] mac2) = ChaCha20Poly1305.decryptWithAD(tempK3, new byte[12], handshakeHash, t);
            // TODO: do mac check
            
            (byte[] decryptKey, byte[] encryptKey) = HmacSha256.ComputeHashes(chainingKey, new byte[0]);
            
            _state.ChainingKey = chainingKey;
            _state.HandshakeHash = handshakeHash;
            _state.SendEncryptionKey = encryptKey;
            _state.ReceiveDecryptionKey = decryptKey;
            _state.SendNonce.Reset();
            _state.ReceiveNonce.Reset();
            _state.HandshakeState = HandshakeState.Finished;
        }

        public void ReadAndWriteHandshake(byte[] inputBuffer, int inputBufferLength, NetworkStream stream)
        {
            if (_state.HandshakeState == HandshakeState.Initialized)
            {
                (_, ECKeyPair ephemeralKey) = ReadActOneRequest(inputBuffer, inputBufferLength);
                var toWrite = ApplyActTwo(ephemeralKey);
                stream.Write(toWrite, 0, toWrite.Length);
            } 
            
            if (_state.HandshakeState == HandshakeState.Act1)
            {
                (byte[] tempK2, ECKeyPair ephemeralKey) = ReadActTwoAnswer(inputBuffer, inputBufferLength);
                var toWrite = ApplyActThree(tempK2, ephemeralKey);
                stream.Write(toWrite, 0, toWrite.Length);
            } 

            if (_state.HandshakeState == HandshakeState.Act2)
            {
                ReadActThreeRequest(inputBuffer, inputBufferLength);
            } 
        }

        public async Task ReadHandshake(NetworkStream stream)
        {
            int bytesRead = -1;

            while (bytesRead != 0 && _state.HandshakeState != HandshakeState.Finished)
            {
                int handshakeSize = _state.HandshakeState == HandshakeState.Act2 ? 66 : 50;
                byte[] handshakeData = await stream.ReadExactly(handshakeSize);
                
                if (handshakeData.Length > 0)
                {
                    ReadAndWriteHandshake(handshakeData, handshakeData.Length, stream);
                }
                else
                {
                    return;
                }
            }
        }
    }
}