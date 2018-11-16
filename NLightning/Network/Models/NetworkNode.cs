using System;
using System.Collections.Generic;
using System.Linq;
using NLightning.Network.Address;
using NLightning.Network.GossipMessages;

namespace NLightning.Network.Models
{
    public class NetworkNode
    {
        public string Id { get; set; }
        public byte[] Features { get; set; }
        public byte[] Signature { get; set; }
        public DateTime Timestamp { get; set; }
        public string Color { get; set; }
        public string Alias { get; set; }
        public string IpV4Address { get; set; }
        public uint IpV4Port { get; set; }
        public string IpV6Address { get; set; }
        public uint IpV6Port { get; set; }
        
        public DateTime CreatedEntity { get; set; }
        public DateTime LastUpdated { get; set; }
        
        public HashSet<NetworkChannel> Node1Channels { get; set; } = new HashSet<NetworkChannel>();
        public HashSet<NetworkChannel> Node2Channels { get; set; } = new HashSet<NetworkChannel>();
        public int ChannelCount => Node1Channels.Count + Node2Channels.Count;
        
        public IEnumerable<NetworkChannel> GetAllChannels() => Node1Channels.Union(Node2Channels);

        public static NetworkNode Create(NodeAnnouncementMessage message)
        {
            NetworkNode networkNode = new NetworkNode();
            networkNode.Id = message.NodeIdHex;
            networkNode.CreatedEntity = DateTime.Now;
            return networkNode.Update(message);
        }
        
        public static NetworkNode Create(string nodeId)
        {
            NetworkNode networkNode = new NetworkNode();
            networkNode.Id = nodeId;
            networkNode.CreatedEntity = DateTime.Now;
            networkNode.Timestamp = DateTime.Now;
            networkNode.LastUpdated = DateTime.Now;
            
            return networkNode;
        }
        
        public NetworkNode Update(NodeAnnouncementMessage message)
        {
            var addresses = message.GetAddresses();
            var ipv4 = addresses.FirstOrDefault(m => m.Type == AddressType.IpV4);
            var ipv6 = addresses.FirstOrDefault(m => m.Type == AddressType.IpV6);
            
            Features = message.Features;
            Alias = message.Alias;
            Color = message.Color;
            Signature = message.Signature;
            Timestamp = message.Timestamp;
            LastUpdated = DateTime.Now;

            if (ipv4 != null)
            {
                IpV4Address = ipv4.Address;
                IpV4Port = ipv4.Port;
            }

            if (ipv6 != null)
            {
                IpV6Address = ipv6.Address;
                IpV6Port = ipv6.Port;
            }

            return this;
        }
    }
}