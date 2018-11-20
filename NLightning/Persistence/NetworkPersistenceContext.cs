using Microsoft.EntityFrameworkCore;
using NLightning.Network.Models;
using NLightning.Peer.Channel;
using NLightning.Peer.Channel.Models;
using NLightning.Wallet.Commitment.Models;

namespace NLightning.Persistence
{
    public class NetworkPersistenceContext : DbContext
    {
        public NetworkPersistenceContext(DbContextOptions<NetworkPersistenceContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BuildNetworkNodeModel(modelBuilder);
            BuildPeerNetworkViewStateModel(modelBuilder);
        }

        private static void BuildPeerNetworkViewStateModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PeerNetworkViewState>()
                .HasKey(c => c.PeerNetworkAddress);
        }

        private static void BuildNetworkNodeModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NetworkNode>()
                .Ignore(n => n.ChannelCount);

            modelBuilder.Entity<NetworkChannel>()
                .HasOne(c => c.Node1)
                .WithMany(n => n.Node1Channels)
                .HasForeignKey(c => c.Node1Id);

            modelBuilder.Entity<NetworkChannel>()
                .HasOne(c => c.Node2)
                .WithMany(n => n.Node2Channels)
                .HasForeignKey(c => c.Node2Id);
        }

        public DbSet<NetworkChannel> NetworkChannels { get; set; }
        public DbSet<NetworkNode> Nodes { get; set; }
        public DbSet<PeerNetworkViewState> PeerStates { get; set; }
    }
}