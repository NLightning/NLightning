using NLightning.Peer.Channel.Establishment.Messages;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Models;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;
using NLightning.Wallet.Commitment;
using NLightning.Wallet.Funding;

namespace NLightning.Peer.Channel.Establishment
{
    public class AcceptChannelMessageHandler
    {
        private readonly IChannelLoggingService _channelLoggingService;
        private readonly IFundingService _fundingService;
        private readonly ICommitmentTransactionService _commitmentService;

        public AcceptChannelMessageHandler(IChannelLoggingService channelLoggingService, IFundingService fundingService, ICommitmentTransactionService commitmentService)
        {
            _channelLoggingService = channelLoggingService;
            _fundingService = fundingService;
            _commitmentService = commitmentService;
        }

        public Message Handle(AcceptChannelMessage acceptMessage, PendingChannel pendingChannel)
        {
            var oldState = LocalChannelState.AcceptChannel;
            var openMessage = pendingChannel.OpenMessage;
            _channelLoggingService.LogPendingChannelInfo(openMessage.TemporaryChannelId.ToHex(), oldState, "Remote accepted channel open");
            FundingTransaction fundingTx = _fundingService.CreateFundingTransaction(openMessage.FundingSatoshis, openMessage.FeeratePerKw, openMessage.FundingPubKey, acceptMessage.FundingPubKey);

            var channel = CreateChannel(pendingChannel.ChannelIndex, fundingTx, openMessage, acceptMessage);
            channel.IsFunder = true;
            
            _commitmentService.CreateInitialCommitmentTransactions(openMessage,acceptMessage, channel, pendingChannel.RevocationKey);
            channel.State = LocalChannelState.FundingCreated;

            var signatureOfRemoteCommitmentTx = channel.RemoteCommitmentTxParameters.LocalSignature;
            FundingCreatedMessage fundingCreatedMessage = new FundingCreatedMessage
            {
                TemporaryChannelId = openMessage.TemporaryChannelId,
                FundingTransactionId = fundingTx.Transaction.GetHash().ToBytes(),
                FundingOutputIndex = fundingTx.FundingOutputIndex,
                Signature = signatureOfRemoteCommitmentTx.ToRawSignature()
            };

            pendingChannel.Channel = channel;
            _channelLoggingService.LogStateUpdate(channel, oldState, additionalData: fundingTx.Transaction.ToString());

            return fundingCreatedMessage;
        }
        
        private LocalChannel CreateChannel(uint index, FundingTransaction fundingTx, OpenChannelMessage openMessage, AcceptChannelMessage acceptMessage)
        {
            LocalChannel channel = new LocalChannel();

            channel.ChannelIndex = index;
            channel.TemporaryChannelId = openMessage.TemporaryChannelId.ToHex();
            channel.LocalChannelParameters = ChannelParameters.CreateFromOpenMessage(openMessage);
            channel.RemoteChannelParameters = ChannelParameters.CreateFromAcceptMessage(acceptMessage);
            channel.FundingSatoshis = openMessage.FundingSatoshis;
            channel.PushMSat = openMessage.PushMSat;
            channel.MinimumDepth = acceptMessage.MinimumDepth;
            channel.FundingTransactionId = fundingTx.Transaction.GetHash().ToString();
            channel.FundingOutputIndex = fundingTx.FundingOutputIndex;
            channel.ChannelId = LocalChannel.DeriveChannelId(fundingTx.Transaction, fundingTx.FundingOutputIndex);
            channel.FeeRatePerKw = openMessage.FeeratePerKw;
            
            return channel;
        }

    }
}