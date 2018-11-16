using System.Collections.Generic;

namespace NLightning.Peer.Channel.Models
{
    public class PersistentPeer
    {
        public PersistentPeer()
        {
        }
        
        public PersistentPeer(string address, bool autoConnect)
        {
            Address = address;
            AutoConnect = autoConnect;
        }

        public int Id { get; set; }
        public string Address { get; set; }
        public bool AutoConnect { get; set; } = true;
        public List<LocalChannel> Channels { get; set; } = new List<LocalChannel>();
    }
}