using NLightning.OnChain.Monitoring;
using NLightning.Peer.Channel.Establishment.Messages;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Logging.Models;
using NLightning.Peer.Channel.Models;
using NLightning.Transport.Messaging.SetupMessages;
using NLightning.Utils;
using NLightning.Wallet.Commitment;
using NLightning.Wallet.Funding;
using Org.BouncyCastle.Asn1.Esf;

namespace NLightning.Peer.Channel.Establishment
{
    public class FundingMessageSignedHandler
    {
        private readonly IChannelLoggingService _channelLoggingService;
        private readonly IFundingService _fundingService;
        private readonly ICommitmentTransactionService _commitmentService;
        private readonly IChannelService _channelService;
        private readonly IBlockchainMonitorService _blockchainMonitorService;

        public FundingMessageSignedHandler(IChannelLoggingService channelLoggingService, IFundingService fundingService,
            ICommitmentTransactionService commitmentService, IChannelService channelService, IBlockchainMonitorService blockchainMonitorService)
        {
            _channelLoggingService = channelLoggingService;
            _fundingService = fundingService;
            _commitmentService = commitmentService;
            _channelService = channelService;
            _blockchainMonitorService = blockchainMonitorService;
        }

        public void Handle(IPeer peer, FundingSignedMessage message, PendingChannel pendingChannel)
        {
            var channel = pendingChannel.Channel;
            var oldState = channel.State;
            var signature = SignatureConverter.RawToTransactionSignature(message.Signature);

            if (!_commitmentService.IsValidRemoteCommitmentSignature(channel, signature))
            {
                _channelLoggingService.LogError(channel, LocalChannelError.InvalidCommitmentSignature, $"Remote {peer.NodeAddress} sent us an invalid commitment signature");
                string error = "Invalid commitment transaction signature.";
                channel.State = LocalChannelState.FundingFailed;
                peer.Messaging.Send(new ErrorMessage(message.ChannelId, error));
                throw new ChannelException(error, channel);
            }
            
            channel.LocalCommitmentTxParameters.RemoteSignature = signature;
            channel.State = LocalChannelState.FundingSigned;
            _channelService.AddChannel(peer, channel);
            _fundingService.BroadcastFundingTransaction(channel);
            _blockchainMonitorService.WatchForTransactionId(channel.FundingTransactionId, (ushort)channel.MinimumDepth);
            _channelService.RemovePendingChannel(pendingChannel);
            _channelLoggingService.LogStateUpdate(channel, oldState);
        }
    }
}