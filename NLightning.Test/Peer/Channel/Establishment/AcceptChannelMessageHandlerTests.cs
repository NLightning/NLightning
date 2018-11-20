using Moq;
using NBitcoin;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.Peer.Channel;
using NLightning.Peer.Channel.Establishment;
using NLightning.Peer.Channel.Establishment.Messages;
using NLightning.Peer.Channel.Models;
using NLightning.Utils.Extensions;
using NLightning.Wallet.Commitment.Models;
using NLightning.Wallet.Funding;
using Xunit;

namespace NLightning.Test.Peer.Channel.Establishment
{
    public class AcceptChannelMessageHandlerTests
    {
        [Fact]
        public void HandleTest()
        {
            var mocks = new ChannelEstablishmentMocks();
            var revocationKey = new ECKeyPair("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65", true);
            var handler = new AcceptChannelMessageHandler(mocks.ChannelLoggingService.Object, mocks.FundingService.Object, mocks.CommTxService.Object);
            
            var localSig = "3045022100a5c01383d3ec646d97e40f44318d49def817fcd61a0ef18008a665b3e151785502203e648efddd5838981ef55ec954be69c4a652d021e6081a100d034de366815e9b".HexToByteArray();
            
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
                FundingPubKey = new ECKeyPair("0245b02f6672c2342fe3ced57118fcf4a0309327e32c335ce494365eb0d15b7200", false),
                RevocationBasepoint = new ECKeyPair("02d91224d91760f477df21d24713b713c681b084e508f48dc77ca14db549ba8ceb", false),
                PaymentBasepoint = new ECKeyPair("022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92", false),
                DelayedPaymentBasepoint = new ECKeyPair("0250d049da6b5832a9f2416df3b0db52da127426c2b70a35ca9c270a72f3f840b5", false),
                HtlcBasepoint = new ECKeyPair("029d100efe40aa3f58985fa12bd0f5c75711449ff4d30adca6f1968a2200bbbf1a", false),
                FirstPerCommitmentPoint = new ECKeyPair("022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92", false),
                ChannelFlags = 1
            };
            
            AcceptChannelMessage acceptChannelMessage = new AcceptChannelMessage()
            {
                TemporaryChannelId = "43497fd7f826957108f4a30fd9cec3aeba79972084e90ead01ea3309000000".HexToByteArray(),
                DustLimitSatoshis = 500,
                MaxHtlcValueInFlightMSat = 50000,
                ChannelReserveSatoshis = 5000,
                HtlcMinimumMSat = 1100,
                ToSelfDelay = 133,
                MaxAcceptedHtlcs = 156,
                FundingPubKey = new ECKeyPair("0299de4bbf495e5bbeb2456c2beb3f40450a3fa41aaa50819ae201f8ad69226bfe", false),
                RevocationBasepoint = new ECKeyPair("022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92", false),
                PaymentBasepoint = new ECKeyPair("02d91224d91760f477df21d24713b713c681b084e508f48dc77ca14db549ba8ceb", false),
                DelayedPaymentBasepoint = new ECKeyPair("0341665cedb568e09f0ab2ab4a28bc2749620deacefb3dce61aac8251c91709d3a", false),
                HtlcBasepoint = new ECKeyPair("0336439e36e2bc1f264c6d3bc6e12db6256389bef2056c32e6267d6e285c2b2122", false),
                FirstPerCommitmentPoint = new ECKeyPair("039360132ab07e7f56d6782a644233da9c4c24845609fcd302cbedd69f69848358", false)
            };
            
            PendingChannel pendingChannel = new PendingChannel(mocks.Peer.Object, openChannelMessage, revocationKey, 0);

            FundingTransaction fundingTransactionMock = mocks.CreateFundingTxMock(openChannelMessage.FundingPubKey, acceptChannelMessage.FundingPubKey);

            mocks.FundingService.Setup(f => f.CreateFundingTransaction(openChannelMessage.FundingSatoshis,
                    openChannelMessage.FeeratePerKw, openChannelMessage.FundingPubKey, acceptChannelMessage.FundingPubKey))
                .Returns(fundingTransactionMock);

            mocks.CommTxService.Setup(f => f.CreateInitialCommitmentTransactions(openChannelMessage, acceptChannelMessage, It.IsAny<LocalChannel>(), revocationKey))
                .Callback<OpenChannelMessage, AcceptChannelMessage, LocalChannel, ECKeyPair>((openMessage, acceptMessage, channel, rkey) =>
                {

                    channel.RemoteCommitmentTxParameters = new CommitmentTransactionParameters();
                    channel.RemoteCommitmentTxParameters.LocalSignature = new TransactionSignature(localSig, SigHash.All);
                });
            
            FundingCreatedMessage fundingCreatedMessage = handler.Handle(acceptChannelMessage, pendingChannel);
            
            Assert.Equal("15fadebd5a68d2d9f216a2dc5531c4dbc977eac0c42ab97899e21d549dddbefa", fundingCreatedMessage.FundingTransactionId.ToHex());
            Assert.Equal(new TransactionSignature(localSig, SigHash.All).ToRawSignature().ToHex(), fundingCreatedMessage.Signature.ToHex());
            Assert.Equal(0, fundingCreatedMessage.FundingOutputIndex);
            Assert.Equal("43497fd7f826957108f4a30fd9cec3aeba79972084e90ead01ea3309000000", fundingCreatedMessage.TemporaryChannelId.ToHex());
        }
    }
}