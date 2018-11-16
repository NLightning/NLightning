using System;
using NLightning.Network.GossipMessages;

namespace NLightning.Network.Models
{
    public class NetworkChannel
    {
        public string Id { get; set; }
        public byte[] Features { get; set; }
        public byte[] ChainHash { get; set; }
        public byte[] ChannelShortId { get; set; }
        
        public DateTime CreatedEntity { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastUpdateMessageTimestamp { get; set; }
        
        public byte[] NodeSignature1 { get; set; }
        public byte[] BitcoinSignature1 { get; set; }
        public byte[] BitcoinKey1 { get; set; }
        public String Node1Id { get; set; }
        public NetworkNode Node1 { get; set; }
        
        public byte[] NodeSignature2 { get; set; }
        public byte[] BitcoinSignature2 { get; set; }
        public byte[] BitcoinKey2 { get; set; }
        public String Node2Id { get; set; }
        public NetworkNode Node2 { get; set; }
        
        public ulong HtlcMinimumSat { get; set; }
        public ulong HtlcMaximumSat { get; set; }
        public uint FeeProportionalMillionths { get; set; }
        public uint FeeBaseMsat { get; set; }
        public ushort CltvExpiryDelta { get; set; }
        public byte MessageFlags { get; set; }
        public byte ChannelFlags { get; set; }

        public static NetworkChannel Create(ChannelAnnouncementMessage announcement, ChannelUpdateMessage update, NetworkNode node1, NetworkNode node2)
        {
            NetworkChannel networkChannel = new NetworkChannel();

            networkChannel.CreatedEntity = DateTime.Now;
            networkChannel.Id = announcement.ShortChannelIdHex;
            networkChannel.Node1 = node1;
            networkChannel.Node2 = node2;
            networkChannel.Node1Id = node1.Id;
            networkChannel.Node2Id = node2.Id;
            networkChannel.ChannelShortId = announcement.ShortChannelId;
            networkChannel.Features = announcement.Features;
            networkChannel.ChainHash = announcement.ChainHash;
            networkChannel.NodeSignature1 = announcement.NodeSignature1;
            networkChannel.NodeSignature2 = announcement.NodeSignature2;
            networkChannel.BitcoinSignature1 = announcement.BitcoinSignature1;
            networkChannel.BitcoinSignature2 = announcement.BitcoinSignature2;
            networkChannel.BitcoinKey1 = announcement.BitcoinKey1.PublicKeyCompressed;
            networkChannel.BitcoinKey2 = announcement.BitcoinKey2.PublicKeyCompressed;
            
            if (update != null)
            {
                networkChannel.Update(update);
            }

            return networkChannel;
        }
        
        public NetworkChannel Update(ChannelUpdateMessage update)
        {
            LastUpdated = DateTime.Now;
            LastUpdateMessageTimestamp = update.Timestamp;
            ChannelFlags = update.ChannelFlags;
            MessageFlags = update.MessageFlags;
            CltvExpiryDelta = update.CltvExpiryDelta;
            FeeBaseMsat = update.FeeBaseMsat;
            FeeProportionalMillionths = update.FeeProportionalMillionths;
            HtlcMaximumSat = update.HtlcMaximumSat;
            HtlcMinimumSat = update.HtlcMinimumSat;
            return this;
        }
    }
}