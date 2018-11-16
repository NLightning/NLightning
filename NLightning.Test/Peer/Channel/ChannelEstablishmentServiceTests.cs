using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NBitcoin;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.OnChain.Client;
using NLightning.OnChain.Monitoring;
using NLightning.Peer;
using NLightning.Peer.Channel;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Models;
using NLightning.Transport;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;
using NLightning.Wallet;
using NLightning.Wallet.Commitment;
using NLightning.Wallet.Funding;
using NLightning.Wallet.KeyDerivation;
using Xunit;

namespace NLightning.Test.Peer.Channel
{
    public class ChannelEstablishmentServiceTests
    {
        [Fact]
        public void InitializeTest()
        {
            var mocks = new Mocks();
            var channel = mocks.CreateChannelMock();

            channel.State = LocalChannelState.FundingLocked;
            mocks.SetupMocks();
            mocks.ChannelService.Setup(c => c.Channels)
                .Returns(() => new List<LocalChannel>() {channel}.AsReadOnly());

            var service = mocks.CreateServiceMock();
            service.Initialize(NetworkParameters.BitcoinTestnet);

            mocks.BlockchainMonitorService
                .Verify(bms => bms.WatchForTransactionId(
                    It.Is<string>(id => id == channel.FundingTransactionId), 
                    It.Is<ushort>(id => id == channel.MinimumDepth)), 
                    Times.Once());
        }

        [Fact]
        public void OpenChannelTest()
        {
            var mocks = new Mocks();
            mocks.SetupMocks();
            
            var service = mocks.CreateServiceMock();
            service.Initialize(NetworkParameters.BitcoinTestnet);
            service.OpenChannel(mocks.Peer.Object, 42000, 100);
            
            mocks.MessagingClient
                .Verify(mock => mock.Send(
                    It.Is<Message>(message => VerifyOpenMessage(message))), Times.Once());
        }

        private bool VerifyOpenMessage(Message message)
        {
            return true;
        }

        private class Mocks
        {
            public Mocks()
            {
                PeerService = new Mock<IPeerService>();
                FundingService = new Mock<IFundingService>();
                ChannelLoggingService = new Mock<IChannelLoggingService>();
                CommTxService = new Mock<ICommitmentTransactionService>();
                ChannelService = new Mock<IChannelService>();
                BlockchainClientService = new Mock<IBlockchainClientService>();
                KeyDerivationService = new Mock<IKeyDerivationService>();
                BlockchainMonitorService = new Mock<IBlockchainMonitorService>();
                WalletService = new Mock<IWalletService>();
                LocalNodeKey = new ECKeyPair("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65", true);
                NodeAddress = NodeAddress.Parse("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991@1.0.0.1:1111");
                LoggerFactory = new LoggerFactory();
                Configuration = new ConfigurationBuilder().Build();
                Peer = new Mock<IPeer>();
                MessagingClient = new Mock<IMessagingClient>();
            }

            public LoggerFactory LoggerFactory { get; set; }
            public NodeAddress NodeAddress { get; set; }
            public IConfigurationRoot Configuration { get; set; }
            public ECKeyPair LocalNodeKey { get; set; }

            public Mock<IMessagingClient> MessagingClient { get; set; }
            public Mock<IPeer> Peer { get; set; }
            public Mock<IPeerService> PeerService { get; set; }
            public Mock<IFundingService> FundingService { get; set; }
            public Mock<IChannelService> ChannelService { get; set; }
            public Mock<IKeyDerivationService> KeyDerivationService { get; set; }
            public Mock<IWalletService> WalletService { get; set; }
            public Mock<IBlockchainMonitorService> BlockchainMonitorService { get; set; }
            public Mock<IBlockchainClientService> BlockchainClientService { get; set; }
            public Mock<ICommitmentTransactionService> CommTxService { get; set; }
            public Mock<IChannelLoggingService> ChannelLoggingService { get; set; }

            public void SetupMocks()
            {
                PeerService.Setup(p => p.IncomingMessageProvider).Returns(() => new Subject<(IPeer, Message)>());
                BlockchainMonitorService.Setup(p => p.ByTransactionIdProvider).Returns(() => new Subject<Transaction>());
                ChannelService.Setup(c => c.Channels).Returns(() => new List<LocalChannel>().AsReadOnly());
                Peer.Setup(c => c.Messaging).Returns(() => MessagingClient.Object);
            }

            public IChannelEstablishmentService CreateServiceMock()
            {
                return new ChannelEstablishmentService(LoggerFactory, Configuration,
                    PeerService.Object, FundingService.Object,
                    ChannelLoggingService.Object, CommTxService.Object,
                    ChannelService.Object, BlockchainClientService.Object,
                    KeyDerivationService.Object, BlockchainMonitorService.Object,
                    WalletService.Object);
            }

            public LocalChannel CreateChannelMock()
            {
                LocalChannel channel = new LocalChannel();
                channel.FundingTransactionId = "a37d424c80fbb1c1649d5f569bd91aafec7939c85f50ee691161113626a22e28";
                channel.FundingOutputIndex = 1;
                channel.MinimumDepth = 3;
                return channel;
            }
        }
    }
}