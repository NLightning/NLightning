using System.Collections.Generic;
using System.Linq;
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
using NLightning.Peer.Channel.ChannelEstablishmentMessages;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Models;
using NLightning.Transport;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;
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

            var revocationKey = new ECKeyPair("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65", true);
            var walletKey = new Key("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65".HexToByteArray());
            
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.DelayBase, 0)).Returns(new ECKeyPair("0250d049da6b5832a9f2416df3b0db52da127426c2b70a35ca9c270a72f3f840b5"));
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.HtlcBase, 0)).Returns(new ECKeyPair("029d100efe40aa3f58985fa12bd0f5c75711449ff4d30adca6f1968a2200bbbf1a"));
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.MultiSig, 0)).Returns(new ECKeyPair("0245b02f6672c2342fe3ced57118fcf4a0309327e32c335ce494365eb0d15b7200"));
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.NodeKey, 0)).Returns(new ECKeyPair("02846726efa57378ad8370acf094f26902a7f1e21903791ef4ab6f989da86679f2"));
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.PaymentBase, 0)).Returns(new ECKeyPair("022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92"));
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.RevocationBase, 0)).Returns(new ECKeyPair("02d91224d91760f477df21d24713b713c681b084e508f48dc77ca14db549ba8ceb"));
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.RevocationRoot, 0)).Returns(revocationKey);
            mocks.KeyDerivationService.Setup(kds => kds.DerivePerCommitmentPoint(It.Is<ECKeyPair>(key => key.PrivateKeyData.SequenceEqual(revocationKey.PrivateKeyData)), 0))
                .Returns(new ECKeyPair("022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92"));

            mocks.WalletService.Setup(ws => ws.ShutdownScriptPubKey).Returns(walletKey.PubKey.GetAddress(NBitcoin.Network.TestNet).ScriptPubKey.ToBytes);
            
            var service = mocks.CreateServiceMock();
            service.Initialize(NetworkParameters.BitcoinTestnet);
            var pendingChannel = service.OpenChannel(mocks.Peer.Object, 42000, 100);
            
            mocks.MessagingClient
                .Verify(mock => mock.Send(
                    It.Is<OpenChannelMessage>(message => VerifyOpenMessage(message))), Times.Once());
            
            mocks.ChannelService.Verify(mock => mock.AddPendingChannel(pendingChannel), Times.Once());
        }

        private bool VerifyOpenMessage(OpenChannelMessage message)
        {
            Assert.Equal(0, message.ChannelFlags);
            Assert.Equal((ulong)42000, message.FundingSatoshis);
            Assert.Equal((ulong)4200, message.ChannelReserveSatoshis);
            Assert.Equal((ulong)546, message.DustLimitSatoshis);
            Assert.Equal((ulong)253, message.FeeratePerKw);
            Assert.Equal((ulong)483, message.MaxAcceptedHtlcs);
            Assert.Equal((ulong)100, message.PushMSat);
            Assert.Equal((ulong)144, message.ToSelfDelay);
            Assert.Equal((ulong)1000, message.HtlcMinimumMSat);
            Assert.Equal((ulong)5000000000, message.MaxHtlcValueInFlightMSat);
            Assert.Equal("43497fd7f826957108f4a30fd9cec3aeba79972084e90ead01ea330900000000".HexToByteArray(), message.ChainHash);
            Assert.Equal("029d100efe40aa3f58985fa12bd0f5c75711449ff4d30adca6f1968a2200bbbf1a", message.HtlcBasepoint.PublicKeyCompressed.ToHex());
            Assert.Equal("022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92", message.PaymentBasepoint.PublicKeyCompressed.ToHex());
            Assert.Equal("02d91224d91760f477df21d24713b713c681b084e508f48dc77ca14db549ba8ceb", message.RevocationBasepoint.PublicKeyCompressed.ToHex());
            Assert.Equal("0250d049da6b5832a9f2416df3b0db52da127426c2b70a35ca9c270a72f3f840b5", message.DelayedPaymentBasepoint.PublicKeyCompressed.ToHex());
            Assert.Equal("0245b02f6672c2342fe3ced57118fcf4a0309327e32c335ce494365eb0d15b7200", message.FundingPubKey.PublicKeyCompressed.ToHex());
            Assert.Equal("022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92", message.FirstPerCommitmentPoint.PublicKeyCompressed.ToHex());
            Assert.Equal("76a914f75d1c854c52abee075916b41cf0ca76fa515b4c88ac", message.ShutdownScriptPubKey.ToHex());
            
            return true;
        }

        [Fact]
        public void OnAcceptChannelMessageTest()
        {
            var mocks = new Mocks();

            mocks.SetupMocks();
            mocks.ChannelService.Setup(c => c.Channels).Returns(() => new List<LocalChannel>() {}.AsReadOnly());

            var service = mocks.CreateServiceMock();
            service.Initialize(NetworkParameters.BitcoinTestnet);
            
            
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
                PeerService.Setup(p => p.ValidationExceptionProvider).Returns(() => new Subject<(IPeer, MessageValidationException)>());
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