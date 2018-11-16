using System.Collections.Generic;
using NBitcoin;
using NLightning.Peer.Channel.Logging.Models;
using NLightning.Utils.Extensions;
using NLightning.Wallet.Commitment.Models;

namespace NLightning.Peer.Channel.Models
{
    public class LocalChannel
    {
        private LocalChannelState _state;
        public int Id { get; set; }
        public string ChannelId { get; set; }
        public string TemporaryChannelId { get; set; }
        public PersistentPeer PersistentPeer { get; set; }
        public bool IsFunder { get; set; }
        public ulong FundingSatoshis { get; set; }
        public ulong PushMSat { get; set; }
        public ulong FeeRatePerKw { get; set; }
        public bool Active { get; set; }
        
        public CommitmentTransactionParameters LocalCommitmentTxParameters { get; set; }
        public CommitmentTransactionParameters RemoteCommitmentTxParameters { get; set; }
        
        public ChannelParameters LocalChannelParameters { get; set; }
        public ChannelParameters RemoteChannelParameters { get; set; }
        
        public uint ChannelIndex { get; set; }
        public List<Htlc> Htlcs { get; set; } = new List<Htlc>();

        public LocalChannelState State
        {
            get => _state;
            set
            {
                if (value < _state)
                {
                    throw new ChannelException($"Invalid Channel State update. Can't update state from {_state} to {value}.");
                }
                
                _state = value;
            }
        }

        public CloseReason CloseReason { get; set; } = CloseReason.None;
        public uint MinimumDepth { get; set; }
        public string FundingTransactionId { get; set; }
        public ushort FundingOutputIndex { get; set; }
        
        public List<LocalChannelLogEntry> Logs { get; set; }
        
        public static string DeriveChannelId(Transaction fundingTransaction, ushort outputIndex)
        {
            var data = outputIndex.GetBytesBigEndian();
            var txId = fundingTransaction.GetHash().ToBytes();

            for (int i = txId.Length - 2; i < txId.Length; i++)
            {
                txId[i] ^= data[i - (txId.Length - 2)];
            }

            return txId.ToHex();
        }
    }
}