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
    public class FundingLockedHandlerTests
    {
        [Fact]
        public void HandleLocalFundingLockedTest()
        {
            var mocks = new ChannelEstablishmentMocks();
            var handler = new FundingMessageLockedHandler(mocks.ChannelLoggingService.Object, 
                mocks.CommTxService.Object, mocks.ChannelService.Object);
            var nextCommPoint = new ECKeyPair("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65", true);
            
            mocks.SetupMocks();

            
            var channel = mocks.CreateChannelMock();
            channel.LocalCommitmentTxParameters = new CommitmentTransactionParameters();
            channel.State = LocalChannelState.FundingSigned;
            channel.ChannelId = "b34fafd163cf765b0997187c20c8fe52dde14d8edd0b4406428e882930bb0723";
            
            mocks.CommTxService.Setup(c => c.GetNextLocalPerCommitmentPoint(channel)).Returns(nextCommPoint);
            
            handler.HandleLocalFundingLocked(mocks.Peer.Object, channel);
            
            Assert.Equal(nextCommPoint, channel.LocalCommitmentTxParameters.NextPerCommitmentPoint);
            Assert.Equal(LocalChannelState.FundingLocked, channel.State);
            
            mocks.MessagingClient.Verify(f => f.Send(It.Is<FundingLockedMessage>((msg) => msg.NextPerCommitmentPoint == nextCommPoint && 
                                                                                          msg.ChannelId.ToHex() == channel.ChannelId)), Times.Once);
        }
        
        [Fact]
        public void HandleLocalFundingLockedNormalOperationTest()
        {
            var mocks = new ChannelEstablishmentMocks();
            var handler = new FundingMessageLockedHandler(mocks.ChannelLoggingService.Object, 
                mocks.CommTxService.Object, mocks.ChannelService.Object);
            var nextCommPoint = new ECKeyPair("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65", true);
            
            mocks.SetupMocks();

            var channel = mocks.CreateChannelMock();
            channel.LocalCommitmentTxParameters = new CommitmentTransactionParameters();
            channel.State = LocalChannelState.FundingLocked;
            channel.ChannelId = "b34fafd163cf765b0997187c20c8fe52dde14d8edd0b4406428e882930bb0723";
            
            mocks.CommTxService.Setup(c => c.GetNextLocalPerCommitmentPoint(channel)).Returns(nextCommPoint);
            
            handler.HandleLocalFundingLocked(mocks.Peer.Object, channel);
            
            Assert.Equal(nextCommPoint, channel.LocalCommitmentTxParameters.NextPerCommitmentPoint);
            Assert.Equal(LocalChannelState.NormalOperation, channel.State);
        }
        
        [Fact]
        public void HandleRemoteFundingLockedTest()
        {
            var mocks = new ChannelEstablishmentMocks();
            var handler = new FundingMessageLockedHandler(mocks.ChannelLoggingService.Object, 
                mocks.CommTxService.Object, mocks.ChannelService.Object);
            var nextCommPoint = new ECKeyPair("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65", true);
            var channel = mocks.CreateChannelMock();
            channel.RemoteCommitmentTxParameters = new CommitmentTransactionParameters();
            channel.State = LocalChannelState.FundingSigned;
            channel.ChannelId = "b34fafd163cf765b0997187c20c8fe52dde14d8edd0b4406428e882930bb0723";
            
            mocks.CommTxService.Setup(c => c.GetNextLocalPerCommitmentPoint(channel)).Returns(nextCommPoint);

            var fundingLockedMessage = new FundingLockedMessage()
            {
                ChannelId = channel.ChannelId.HexToByteArray(),
                NextPerCommitmentPoint = nextCommPoint
            };
            
            handler.HandleRemoteFundingLocked(mocks.Peer.Object, fundingLockedMessage, channel);
            
            Assert.Equal(nextCommPoint, channel.RemoteCommitmentTxParameters.NextPerCommitmentPoint);
            Assert.Equal(LocalChannelState.FundingLocked, channel.State);
        }
        
                
        [Fact]
        public void HandleRemoteFundingLockedNormalOperationTest()
        {
            var mocks = new ChannelEstablishmentMocks();
            var handler = new FundingMessageLockedHandler(mocks.ChannelLoggingService.Object, 
                mocks.CommTxService.Object, mocks.ChannelService.Object);
            var nextCommPoint = new ECKeyPair("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65", true);
            var channel = mocks.CreateChannelMock();
            channel.RemoteCommitmentTxParameters = new CommitmentTransactionParameters();
            channel.State = LocalChannelState.FundingLocked;
            channel.ChannelId = "b34fafd163cf765b0997187c20c8fe52dde14d8edd0b4406428e882930bb0723";
            
            mocks.CommTxService.Setup(c => c.GetNextLocalPerCommitmentPoint(channel)).Returns(nextCommPoint);

            var fundingLockedMessage = new FundingLockedMessage()
            {
                ChannelId = channel.ChannelId.HexToByteArray(),
                NextPerCommitmentPoint = nextCommPoint
            };
            
            handler.HandleRemoteFundingLocked(mocks.Peer.Object, fundingLockedMessage, channel);
            
            Assert.Equal(nextCommPoint, channel.RemoteCommitmentTxParameters.NextPerCommitmentPoint);
            Assert.Equal(LocalChannelState.NormalOperation, channel.State);
        }
        
        [Fact]
        public void HandleRemoteFundingLockedResendTest()
        {
            var mocks = new ChannelEstablishmentMocks();
            var handler = new FundingMessageLockedHandler(mocks.ChannelLoggingService.Object, 
                mocks.CommTxService.Object, mocks.ChannelService.Object);
            var nextCommPoint = new ECKeyPair("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65", true);
            var channel = mocks.CreateChannelMock();
            channel.RemoteCommitmentTxParameters = new CommitmentTransactionParameters();
            channel.State = LocalChannelState.NormalOperation;
            channel.ChannelId = "b34fafd163cf765b0997187c20c8fe52dde14d8edd0b4406428e882930bb0723";
            
            mocks.CommTxService.Setup(c => c.GetNextLocalPerCommitmentPoint(channel)).Returns(nextCommPoint);

            var fundingLockedMessage = new FundingLockedMessage()
            {
                ChannelId = channel.ChannelId.HexToByteArray(),
                NextPerCommitmentPoint = nextCommPoint
            };
            
            mocks.SetupMocks();
            
            handler.HandleRemoteFundingLocked(mocks.Peer.Object, fundingLockedMessage, channel);

            mocks.MessagingClient.Verify(f => f.Send(It.Is<FundingLockedMessage>((msg) => msg.NextPerCommitmentPoint == nextCommPoint && 
                                                                                          msg.ChannelId.ToHex() == channel.ChannelId)), Times.Once);
        }
    }
}