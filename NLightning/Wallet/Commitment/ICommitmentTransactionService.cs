using NBitcoin;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.Peer.Channel.ChannelEstablishmentMessages;
using NLightning.Peer.Channel.Models;
using NLightning.Wallet.Funding;

namespace NLightning.Wallet.Commitment
{
    public interface ICommitmentTransactionService
    {
        void Initialize(NetworkParameters networkParameters);
        void CreateInitialCommitmentTransactions(OpenChannelMessage openMessage, AcceptChannelMessage acceptMessage, LocalChannel channel, ECKeyPair localRevocationKey);

        ECKeyPair GetNextLocalPerCommitmentPoint(LocalChannel channel);
        void UpdateRemotePerCommitmentPoint(LocalChannel localChannel, ECKeyPair nextPerCommitmentPoint);
        bool IsValidRemoteCommitmentSignature(LocalChannel channel, TransactionSignature signature);
    }
}