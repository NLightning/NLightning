using Microsoft.EntityFrameworkCore;
using NLightning.Network.Models;
using NLightning.OnChain.Monitoring.Models;
using NLightning.Peer.Channel.Logging.Models;
using NLightning.Peer.Channel.Models;
using NLightning.Utils;
using NLightning.Wallet.Commitment.Models;

namespace NLightning.Persistence
{
    public class LocalPersistenceContext : DbContext
    {
        public LocalPersistenceContext(DbContextOptions<LocalPersistenceContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            BuildLocalChannelModel(modelBuilder);
            BuildLocalChannelLogEntryModel(modelBuilder);
            BuildPersistentPeerModel(modelBuilder);
            BuildHtlcModel(modelBuilder);
            BuildChannelParametersModel(modelBuilder);
            BuildCommitmentTransactionParametersModel(modelBuilder);
            BuildSpendingTransactionLookupModel(modelBuilder);
        }

        private static void BuildLocalChannelModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LocalChannel>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<LocalChannel>()
                .HasIndex(c => c.ChannelId)
                .IsUnique();

            modelBuilder.Entity<LocalChannel>()
                .Ignore(c => c.Active);
            
            modelBuilder.Entity<LocalChannel>()
                .HasOne(c => c.LocalChannelParameters);

            modelBuilder.Entity<LocalChannel>()
                .HasOne(c => c.RemoteChannelParameters);

            modelBuilder.Entity<LocalChannel>()
                .HasOne(c => c.LocalCommitmentTxParameters);

            modelBuilder.Entity<LocalChannel>()
                .HasOne(c => c.PersistentPeer)
                .WithMany(p => p.Channels);

            modelBuilder.Entity<LocalChannel>()
                .Property(p => p.State)
                .HasConversion<string>();

            modelBuilder.Entity<LocalChannel>()
                .Property(p => p.CloseReason)
                .HasConversion<string>();

            modelBuilder.Entity<LocalChannel>()
                .HasOne(c => c.RemoteCommitmentTxParameters);

            modelBuilder.Entity<LocalChannel>()
                .HasMany(c => c.Htlcs);
        }

        private static void BuildSpendingTransactionLookupModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SpendingTransactionLookup>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<SpendingTransactionLookup>()
                .HasIndex(c => c.TransactionId);
        }

        private static void BuildCommitmentTransactionParametersModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();
            
            var ecKeyPairValueConverter = new ECKeyPairValueConverter();
            var transactionSignatureConverter = new TransactionSignatureConverter();
            
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.HtlcBasepoint)
                .HasConversion(ecKeyPairValueConverter);
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.PaymentBasepoint)
                .HasConversion(ecKeyPairValueConverter);
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.RevocationBasepoint)
                .HasConversion(ecKeyPairValueConverter);
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.RevocationKey)
                .HasConversion(ecKeyPairValueConverter);
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.DelayedPaymentBasepoint)
                .HasConversion(ecKeyPairValueConverter);
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.FundingKey)
                .HasConversion(ecKeyPairValueConverter);
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.HtlcPublicKey)
                .HasConversion(ecKeyPairValueConverter);
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.PaymentPublicKey)
                .HasConversion(ecKeyPairValueConverter);
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.PerCommitmentKey)
                .HasConversion(ecKeyPairValueConverter);
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.RevocationPublicKey)
                .HasConversion(ecKeyPairValueConverter);
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.DelayedPaymentPublicKey)
                .HasConversion(ecKeyPairValueConverter);
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.NextPerCommitmentPoint)
                .HasConversion(ecKeyPairValueConverter);
            
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.LocalSignature)
                .HasConversion(transactionSignatureConverter);
            
            modelBuilder.Entity<CommitmentTransactionParameters>()
                .Property(p => p.RemoteSignature)
                .HasConversion(transactionSignatureConverter);
        }

        private static void BuildChannelParametersModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChannelParameters>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();
        }

        private static void BuildHtlcModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Htlc>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();
        }

        private static void BuildPersistentPeerModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersistentPeer>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<PersistentPeer>()
                .HasIndex(c => c.Address)
                .IsUnique();
        }

        private static void BuildLocalChannelLogEntryModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LocalChannelLogEntry>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<LocalChannelLogEntry>()
                .HasOne(log => log.Channel)
                .WithMany(c => c.Logs)
                .HasForeignKey(log => log.ChannelId)
                .IsRequired(false);

            modelBuilder.Entity<LocalChannelLogEntry>()
                .Property(e => e.OldState)
                .HasConversion<string>();

            modelBuilder.Entity<LocalChannelLogEntry>()
                .Property(e => e.State)
                .HasConversion<string>();

            modelBuilder.Entity<LocalChannelLogEntry>()
                .Property(e => e.EntryType)
                .HasConversion<string>();

            modelBuilder.Entity<LocalChannelLogEntry>()
                .Property(e => e.ErrorText)
                .HasMaxLength(1024 * 32);

            modelBuilder.Entity<LocalChannelLogEntry>()
                .Property(e => e.ChannelData)
                .HasMaxLength(1024 * 32);

            modelBuilder.Entity<LocalChannelLogEntry>()
                .Property(e => e.DebugData)
                .HasMaxLength(1024 * 32);

            modelBuilder.Entity<LocalChannelLogEntry>()
                .Property(e => e.AdditionalData)
                .HasMaxLength(1024 * 32);
        }

        public DbSet<PersistentPeer> Peers { get; set; }
        public DbSet<LocalChannel> LocalChannels { get; set; }
        public DbSet<LocalChannelLogEntry> LocalChannelLogs { get; set; }
        public DbSet<Htlc> Htlcs { get; set; }
        public DbSet<ChannelParameters> ChannelParameters { get; set; }
        public DbSet<CommitmentTransactionParameters> CommitmentTransactionParameters { get; set; }
        public DbSet<SpendingTransactionLookup> SpendingTransactionLookups { get; set; }
    }
}