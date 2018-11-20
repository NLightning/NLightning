using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.Network.GossipMessages;
using NLightning.OnChain;
using NLightning.OnChain.Client;
using NLightning.OnChain.Monitoring;
using NLightning.Peer;
using NLightning.Peer.AutoConnect;
using NLightning.Peer.Channel;
using NLightning.Peer.Channel.ChannelCloseMessages;
using NLightning.Peer.Channel.CommitmentMessages;
using NLightning.Peer.Channel.Establishment;
using NLightning.Peer.Channel.Establishment.Messages;
using NLightning.Peer.Channel.HtlcMessages;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Penalty;
using NLightning.Persistence;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;
using NLightning.Wallet;
using NLightning.Wallet.Commitment;
using NLightning.Wallet.Funding;
using NLightning.Wallet.KeyDerivation;

namespace NLightning.Node
{
    public class LightningNode : INode
    {
        private readonly ECKeyPair _walletKey;
        
        public LightningNode(ECKeyPair lightningKey, ECKeyPair walletKey, NetworkParameters networkParameters)
        {
            LocalKey = lightningKey;
            NetworkParameters = networkParameters;
            _walletKey = walletKey;
        }

        public IServiceProvider Services { get; private set; }
        public ECKeyPair LocalKey { get; }
        public NetworkParameters NetworkParameters { get; }
        
        public void ConfigureServices(Action<IServiceCollection> configureServices)
        {
            var serviceCollection = new ServiceCollection();

            configureServices?.Invoke(serviceCollection);

            serviceCollection.TryAddSingleton(BuildDefaultConfiguration());
            serviceCollection.TryAddSingleton<ILoggerFactory, LoggerFactory>();
            serviceCollection.TryAddSingleton<IPersistenceService, PersistenceService>();
            serviceCollection.TryAddSingleton<IBlockchainService, BlockchainService>();
            serviceCollection.TryAddSingleton<IBlockchainMonitorService, BlockchainMonitorService>();
            serviceCollection.TryAddSingleton<IPenaltyService, PenaltyService>();
            serviceCollection.TryAddSingleton<IWalletService, WalletService>();
            serviceCollection.TryAddSingleton<IKeyDerivationService, KeyDerivationService>();
            serviceCollection.TryAddSingleton<IFundingService, FundingService>();
            serviceCollection.TryAddSingleton<ICommitmentTransactionService, CommitmentTransactionService>();
            serviceCollection.TryAddSingleton<IPeerService, PeerService>();
            serviceCollection.TryAddSingleton<IPeerAutoConnect, PeerAutoConnectService>();
            serviceCollection.TryAddSingleton<INetworkViewService, NetworkViewService>();
            serviceCollection.TryAddSingleton<INetworkViewSyncService, NetworkViewSyncService>();
            serviceCollection.TryAddSingleton<IChannelLoggingService, ChannelLoggingService>();
            serviceCollection.TryAddSingleton<IChannelStateService, ChannelStateService>();
            serviceCollection.TryAddSingleton<IChannelMessageLoggingService, ChannelMessageLoggingService>();
            serviceCollection.TryAddSingleton<IChannelService, ChannelService>();
            serviceCollection.TryAddSingleton<IChannelEstablishmentService, ChannelEstablishmentService>();
            serviceCollection.TryAddSingleton<IChannelReestablishmentService, ChannelReestablishmentService>();
            serviceCollection.TryAddSingleton<IChannelMonitoringService, ChannelMonitoringService>();
            serviceCollection.TryAddSingleton<IChannelCloseService, ChannelCloseService>();
            serviceCollection.TryAddSingleton<IUnilateralCloseService, UnilateralCloseService>();

            serviceCollection.AddSingleton<IMessageValidator, NodeAnnouncementMessageValidator>();
            serviceCollection.AddSingleton<IMessageValidator, ChannelAnnouncementMessageValidator>();
            serviceCollection.AddSingleton<IMessageValidator, ChannelUpdateMessageValidator>();
            serviceCollection.AddSingleton<IMessageValidator, UpdateAddHtlcMessageValidator>();
            serviceCollection.AddSingleton<IMessageValidator, AcceptChannelMessageValidator>();
            serviceCollection.AddSingleton<IMessageValidator, ChannelReestablishMessageValidator>();
            serviceCollection.AddSingleton<IMessageValidator, FundingCreatedMessageValidator>();
            serviceCollection.AddSingleton<IMessageValidator, FundingLockedMessageValidator>();
            serviceCollection.AddSingleton<IMessageValidator, FundingSignedMessageValidator>();
            serviceCollection.AddSingleton<IMessageValidator, OpenChannelMessageValidator>();
            serviceCollection.AddSingleton<IMessageValidator, ClosingSignedMessageValidator>();
            serviceCollection.AddSingleton<IMessageValidator, ShutdownMessageValidator>();
            Services = serviceCollection.BuildServiceProvider();
        }
        
        public void Initialize()
        {
            Services.GetService<IMessageValidator, ChannelAnnouncementMessageValidator>().Initialize(NetworkParameters);
            Services.GetService<IMessageValidator, ChannelUpdateMessageValidator>().Initialize(NetworkParameters);
            
            Services.GetService<IPersistenceService>().Initialize();
            Services.GetService<IChannelMessageLoggingService>().Initialize();
            Services.GetService<IBlockchainService>().Initialize(NetworkParameters);
            Services.GetService<IBlockchainMonitorService>().Initialize(NetworkParameters);
            Services.GetService<IWalletService>().Initialize(_walletKey, NetworkParameters);
            Services.GetService<IKeyDerivationService>().Initialize(NetworkParameters);
            Services.GetService<IFundingService>().Initialize(NetworkParameters);
            Services.GetService<ICommitmentTransactionService>().Initialize(NetworkParameters);
            Services.GetService<IPenaltyService>().Initialize(NetworkParameters);
            Services.GetService<IBlockchainClientService>().Initialize(_walletKey, NetworkParameters);
            Services.GetService<INetworkViewService>().Initialize();
            Services.GetService<INetworkViewSyncService>().Initialize(NetworkParameters);
            Services.GetService<IPeerService>().Initialize(LocalKey);
            Services.GetService<IChannelStateService>().Initialize();
            Services.GetService<IChannelService>().Initialize(NetworkParameters);
            Services.GetService<IChannelEstablishmentService>().Initialize(NetworkParameters);
            Services.GetService<IChannelReestablishmentService>().Initialize();
            Services.GetService<IChannelMonitoringService>().Initialize(NetworkParameters);
            Services.GetService<IChannelCloseService>().Initialize(NetworkParameters);
            Services.GetService<IUnilateralCloseService>().Initialize(NetworkParameters);
            Services.GetService<IPeerAutoConnect>().Initialize(NetworkParameters);
        }
       
        private IConfiguration BuildDefaultConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("nlightning.json", true, true);

            return configurationBuilder.Build();
        }
    }
}