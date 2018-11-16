using System;

namespace NLightning.Peer
{
    public class PeerException : Exception
    {
        public IPeer Peer { get; }

        public PeerException(string message, IPeer peer = null, Exception innerException = null) : base(message, innerException)
        {
            Peer = peer;
        }
    }
}