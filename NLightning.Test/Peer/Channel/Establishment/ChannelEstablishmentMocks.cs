using System.Collections.Generic;
using System.Linq;
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
using NLightning.Peer.Channel.Establishment;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Models;
using NLightning.Transport;
using NLightning.Transport.Messaging;
using NLightning.Wallet;
using NLightning.Wallet.Commitment;
using NLightning.Wallet.Funding;
using NLightning.Wallet.KeyDerivation;

namespace NLightning.Test.Peer.Channel.Establishment
{
    public class ChannelEstablishmentMocks
    {
        public ChannelEstablishmentMocks()
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
        public Subject<(IPeer Peer, Message Message)> IncomingMessageProviderMock { get; } = new Subject<(IPeer Peer, Message Message)>();
        
        public void SetupMocks()
        {
            PeerService.Setup(p => p.IncomingMessageProvider).Returns(() => IncomingMessageProviderMock);
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

        public FundingTransaction CreateFundingTxMock(ECKeyPair pubKey1, ECKeyPair pubKey2)
        {
            PubKey fundingPubKey1 = pubKey1.ToPubKey();
            PubKey fundingPubKey2 = pubKey2.ToPubKey();
            var secret = Key.Parse("cRCH7YNcarfvaiY1GWUKQrRGmoezvfAiqHtdRvxe16shzbd7LDMz", NBitcoin.Network.TestNet);
            var input = Transaction.Parse("01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff03510101ffffffff0100f2052a010000001976a9143ca33c2e4446f4a305f23c80df8ad1afdcf652f988ac00000000", NBitcoin.Network.TestNet);
            var multiSigPubKey = PayToMultiSigTemplate.Instance.GenerateScriptPubKey(2, fundingPubKey1, fundingPubKey2);
            var coins = input.Outputs.AsCoins().ToList();
             
            TransactionBuilder builder = new TransactionBuilder();
            Transaction unsigned = 
                builder
                    .AddCoins(coins)
                    .Send(multiSigPubKey.WitHash.ScriptPubKey, Money.Satoshis(10000000))
                    .SendFees(Money.Satoshis(13920))
                    .SetChange(secret.PubKey.WitHash)
                    .SetConsensusFactory(NBitcoin.Network.TestNet)
                    .BuildTransaction(sign: false);

            return new FundingTransaction(unsigned, 0, coins, fundingPubKey1.ScriptPubKey.ToBytes());
        }
    }
}