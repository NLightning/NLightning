using Moq;
using NBitcoin;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.Peer.Channel;
using NLightning.Peer.Channel.Establishment;
using NLightning.Peer.Channel.Establishment.Messages;
using NLightning.Peer.Channel.Models;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;
using NLightning.Wallet.Commitment.Models;
using Xunit;

namespace NLightning.Test.Peer.Channel.Establishment
{
    public class FundingSignedHandlerTests
    {
        [Fact]
        public void HandleTest()
        {
            var mocks = new ChannelEstablishmentMocks();
            var revocationKey = new ECKeyPair("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65", true);
            var handler = new FundingMessageSignedHandler(mocks.ChannelLoggingService.Object, mocks.FundingService.Object, 
                mocks.CommTxService.Object, mocks.ChannelService.Object, mocks.BlockchainMonitorService.Object);

            OpenChannelMessage openChannelMessage = new OpenChannelMessage()
            {
                ChainHash = NetworkParameters.BitcoinTestnet.ChainHash,
                TemporaryChannelId = "43497fd7f826957108f4a30fd9cec3aeba79972084e90ead01ea3309000000".HexToByteArray(),
                FundingSatoshis = 420000,
                PushMSat = 1000,
                DustLimitSatoshis = 500,
                MaxHtlcValueInFlightMSat = 50000,
                ChannelReserveSatoshis = 5000,
                HtlcMinimumMSat = 1100,
                FeeratePerKw = 150,
                ToSelfDelay = 133,
                MaxAcceptedHtlcs = 156,
                FundingPubKey = new ECKeyPair("0250d049da6b5832a9f2416df3b0db52da127426c2b70a35ca9c270a72f3f840b5", false),
                RevocationBasepoint = new ECKeyPair("022ecc432552ff86d053514ffb133d3025fb14c39aa5ae2a5169b0367174cabfa4", false),
                PaymentBasepoint = new ECKeyPair("029d100efe40aa3f58985fa12bd0f5c75711449ff4d30adca6f1968a2200bbbf1a", false),
                DelayedPaymentBasepoint = new ECKeyPair("0250d049da6b5832a9f2416df3b0db52da127426c2b70a35ca9c270a72f3f840b5", false),
                HtlcBasepoint = new ECKeyPair("03d029229db8f594adcd545b4a42acbb1013286908d2905fa05c9a4e2083fe3fe2", false),
                FirstPerCommitmentPoint = new ECKeyPair("02846726efa57378ad8370acf094f26902a7f1e21903791ef4ab6f989da86679f2", false),
                ChannelFlags = 1
            };
            
//            AcceptChannelMessage acceptChannelMessage = new AcceptChannelMessage()
//            {
//                TemporaryChannelId = "43497fd7f826957108f4a30fd9cec3aeba79972084e90ead01ea3309000000".HexToByteArray(),
//                DustLimitSatoshis = 500,
//                MaxHtlcValueInFlightMSat = 50000,
//                ChannelReserveSatoshis = 5000,
//                HtlcMinimumMSat = 1100,
//                ToSelfDelay = 133,
//                MaxAcceptedHtlcs = 156,
//                FundingPubKey = new ECKeyPair("0299de4bbf495e5bbeb2456c2beb3f40450a3fa41aaa50819ae201f8ad69226bfe", false),
//                RevocationBasepoint = new ECKeyPair("022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92", false),
//                PaymentBasepoint = new ECKeyPair("02d91224d91760f477df21d24713b713c681b084e508f48dc77ca14db549ba8ceb", false),
//                DelayedPaymentBasepoint = new ECKeyPair("0341665cedb568e09f0ab2ab4a28bc2749620deacefb3dce61aac8251c91709d3a", false),
//                HtlcBasepoint = new ECKeyPair("0336439e36e2bc1f264c6d3bc6e12db6256389bef2056c32e6267d6e285c2b2122", false),
//                FirstPerCommitmentPoint = new ECKeyPair("039360132ab07e7f56d6782a644233da9c4c24845609fcd302cbedd69f69848358", false)
//            };

            var signature = "2f26e967305b4d422116a7c876d338bc4298263f329a54c0d1655f55d594de4955356386addd0bc15ced36dacece0af439694195654c838769583409eeac5d3f".HexToByteArray(); 
            
            FundingSignedMessage fundingSignedMessage = new FundingSignedMessage()
            {
                ChannelId = "43497fd7f826957108f4a30fd9cec3aeba79972084e90ead01ea3309000000".HexToByteArray(),
                Signature = signature
            };
            
            PendingChannel pendingChannel = new PendingChannel(mocks.Peer.Object, openChannelMessage, revocationKey, 0);

            pendingChannel.Channel = mocks.CreateChannelMock();
            pendingChannel.Channel.LocalCommitmentTxParameters = new CommitmentTransactionParameters();
            pendingChannel.Channel.FundingTransactionId = "txId";
            
            mocks.CommTxService.Setup(c => c.IsValidRemoteCommitmentSignature(pendingChannel.Channel, It.IsAny<TransactionSignature>())).Returns(true);
            
            handler.Handle(mocks.Peer.Object, fundingSignedMessage, pendingChannel);
            
            Assert.Equal(LocalChannelState.FundingSigned, pendingChannel.Channel.State);
            Assert.Equal(signature, pendingChannel.Channel.LocalCommitmentTxParameters.RemoteSignature.ToRawSignature());
            
            mocks.FundingService.Verify(f => f.BroadcastFundingTransaction(pendingChannel.Channel), Times.Once);
            mocks.BlockchainMonitorService.Verify(m => m.WatchForTransactionId(pendingChannel.Channel.FundingTransactionId, 3), Times.Once);
            mocks.ChannelService.Verify(c => c.RemovePendingChannel(pendingChannel), Times.Once);
        }
        
        [Fact]
        public void HandleInvalidRemoteSignatureTest()
        {
            var mocks = new ChannelEstablishmentMocks();
            var revocationKey = new ECKeyPair("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65", true);
            var handler = new FundingMessageSignedHandler(mocks.ChannelLoggingService.Object, mocks.FundingService.Object, 
                mocks.CommTxService.Object, mocks.ChannelService.Object, mocks.BlockchainMonitorService.Object);

            OpenChannelMessage openChannelMessage = new OpenChannelMessage()
            {
                ChainHash = NetworkParameters.BitcoinTestnet.ChainHash,
                TemporaryChannelId = "43497fd7f826957108f4a30fd9cec3aeba79972084e90ead01ea3309000000".HexToByteArray(),
                FundingSatoshis = 420000,
                PushMSat = 1000,
                DustLimitSatoshis = 500,
                MaxHtlcValueInFlightMSat = 50000,
                ChannelReserveSatoshis = 5000,
                HtlcMinimumMSat = 1100,
                FeeratePerKw = 150,
                ToSelfDelay = 133,
                MaxAcceptedHtlcs = 156,
                FundingPubKey = new ECKeyPair("0250d049da6b5832a9f2416df3b0db52da127426c2b70a35ca9c270a72f3f840b5", false),
                RevocationBasepoint = new ECKeyPair("022ecc432552ff86d053514ffb133d3025fb14c39aa5ae2a5169b0367174cabfa4", false),
                PaymentBasepoint = new ECKeyPair("029d100efe40aa3f58985fa12bd0f5c75711449ff4d30adca6f1968a2200bbbf1a", false),
                DelayedPaymentBasepoint = new ECKeyPair("0250d049da6b5832a9f2416df3b0db52da127426c2b70a35ca9c270a72f3f840b5", false),
                HtlcBasepoint = new ECKeyPair("03d029229db8f594adcd545b4a42acbb1013286908d2905fa05c9a4e2083fe3fe2", false),
                FirstPerCommitmentPoint = new ECKeyPair("02846726efa57378ad8370acf094f26902a7f1e21903791ef4ab6f989da86679f2", false),
                ChannelFlags = 1
            };
            
            var signature = "2f26e967305b4d422116a7c876d338bc4298263f329a54c0d1655f55d594de4955356386addd0bc15ced36dacece0af439694195654c838769583409eeac5d3f".HexToByteArray(); 
            
            FundingSignedMessage fundingSignedMessage = new FundingSignedMessage()
            {
                ChannelId = "43497fd7f826957108f4a30fd9cec3aeba79972084e90ead01ea3309000000".HexToByteArray(),
                Signature = signature
            };
            
            PendingChannel pendingChannel = new PendingChannel(mocks.Peer.Object, openChannelMessage, revocationKey, 0);

            pendingChannel.Channel = mocks.CreateChannelMock();
            pendingChannel.Channel.LocalCommitmentTxParameters = new CommitmentTransactionParameters();
            
            mocks.CommTxService.Setup(c => c.IsValidRemoteCommitmentSignature(pendingChannel.Channel, It.IsAny<TransactionSignature>())).Returns(false);
            mocks.Peer.Setup(m => m.Messaging).Returns(new Mock<IMessagingClient>().Object);
            
            Assert.Throws<ChannelException>(() => 
                handler.Handle(mocks.Peer.Object, fundingSignedMessage, pendingChannel));
        }
    }
}