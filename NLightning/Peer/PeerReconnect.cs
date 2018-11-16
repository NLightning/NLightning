using System;
using NLightning.Transport;

namespace NLightning.Peer
{
    public class PeerReconnect
    {
        public PeerReconnect(NodeAddress nodeAddress)
        {
            NodeAddress = nodeAddress;
        }
        
        public NodeAddress NodeAddress { get; }
        public DateTime LastReconnect { get; private set; }
        public int FailedReconnects { get; private set; }

        public void AddFailedReconnect()
        {
            FailedReconnects++;
            LastReconnect = DateTime.Now;
        }
    }
}