using NLightning.Cryptography;

namespace NLightning.Transport
{
    public class TransportState
    {
        public TransportState()
        {
            IsInitiatingNode = false;
            LocalKeyPair = null;
            RemoteAddress = null;
        }
        
        public TransportState(bool isInitiatingNode, ECKeyPair localKeyPair, NodeAddress remoteAddress)
        {
            IsInitiatingNode = isInitiatingNode;
            LocalKeyPair = localKeyPair;
            RemoteAddress = remoteAddress;
        }

        public ECKeyPair LocalKeyPair { get; } 
        public NodeAddress RemoteAddress { get; }
        public bool IsInitiatingNode { get; }
        public HandshakeState HandshakeState { get; set; } = HandshakeState.Uninitialized;
        public byte[] ChainingKey { get; set; }
        public byte[] HandshakeHash { get; set; }
        public byte[] SendEncryptionKey { get; set; }
        public byte[] ReceiveDecryptionKey { get; set; }
        public ECKeyPair EphemeralKeyPair { get; set; }
        public Nonce SendNonce { get; } = new Nonce();
        public Nonce ReceiveNonce { get; } = new Nonce();
        public byte[] TempKey2 { get; set; }

        public void RotateReceiveKey()
        {
            (byte[] newChainingKey, byte[] newReceiveKey) = HmacSha256.ComputeHashes(ChainingKey, ReceiveDecryptionKey);
            ChainingKey = newChainingKey;
            ReceiveDecryptionKey = newReceiveKey;
            ReceiveNonce.Reset();
        }
        
        public void RotateSendKey()
        {
            (byte[] newChainingKey, byte[] newSendKey) = HmacSha256.ComputeHashes(ChainingKey, SendEncryptionKey);
            ChainingKey = newChainingKey;
            SendEncryptionKey = newSendKey;
            SendNonce.Reset();
        }
    }
}