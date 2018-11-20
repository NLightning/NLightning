using NLightning.Peer.Channel.Establishment.Messages;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Models;
using NLightning.Utils.Extensions;
using NLightning.Wallet.Commitment;

namespace NLightning.Peer.Channel.Establishment
{
    public class FundingMessageLockedHandler
    {
        private readonly IChannelLoggingService _channelLoggingService;
        private readonly ICommitmentTransactionService _commitmentService;
        private readonly IChannelService _channelService;

        public FundingMessageLockedHandler(IChannelLoggingService channelLoggingService, ICommitmentTransactionService commitmentService, IChannelService channelService)
        {
            _channelLoggingService = channelLoggingService;
            _commitmentService = commitmentService;
            _channelService = channelService;
        }

        public void HandleRemoteFundingLocked(IPeer peer, FundingLockedMessage message, LocalChannel channel)
        {
            if (channel.State == LocalChannelState.NormalOperation && channel.RemoteCommitmentTxParameters.TransactionNumber == 0)
            {
                _channelLoggingService.LogInfo(channel, $"Remote sent us a {nameof(FundingLockedMessage)} but we are already in Normal Operation state. " +
                                                        "We will answer with a funding locked message.");
                SendFundingLocked(peer, channel);
                return;
            }
            
            if (channel.State != LocalChannelState.FundingSigned && channel.State != LocalChannelState.FundingLocked)
            {
                _channelLoggingService.LogWarning(channel, $"Remote sent us a {nameof(FundingLockedMessage)}, but the current state is {channel.State}");
                return;
            }
            
            channel.State = channel.State == LocalChannelState.FundingLocked ? LocalChannelState.NormalOperation : LocalChannelState.FundingLocked;
            channel.RemoteCommitmentTxParameters.NextPerCommitmentPoint = message.NextPerCommitmentPoint;
        }

        public void HandleLocalFundingLocked(IPeer peer, LocalChannel channel)
        {
            if (channel.State == LocalChannelState.NormalOperation)
            {
                return;
            }
            
            channel.State = channel.State == LocalChannelState.FundingLocked ? LocalChannelState.NormalOperation : LocalChannelState.FundingLocked;
            channel.LocalCommitmentTxParameters.NextPerCommitmentPoint = _commitmentService.GetNextLocalPerCommitmentPoint(channel);
            
            _channelService.UpdateChannel(channel);
            SendFundingLocked(peer, channel);
        }        
        
        private void SendFundingLocked(IPeer peer, LocalChannel channel)
        {
            FundingLockedMessage message = new FundingLockedMessage();
            message.ChannelId = channel.ChannelId.HexToByteArray();
            message.NextPerCommitmentPoint = _commitmentService.GetNextLocalPerCommitmentPoint(channel);
            peer.Messaging.Send(message);
        }

    }
}