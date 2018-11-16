using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.OnChain;
using NLightning.Peer.Channel.ChannelEstablishmentMessages;
using NLightning.Peer.Channel.Logging;
using NLightning.Peer.Channel.Models;
using NLightning.Utils.Extensions;
using NLightning.Wallet.Commitment.Models;
using NLightning.Wallet.Funding;
using NLightning.Wallet.KeyDerivation;

namespace NLightning.Wallet.Commitment
{
    public class CommitmentTransactionService : ICommitmentTransactionService
    {

        private readonly IKeyDerivationService _derivationService;
        private readonly IChannelLoggingService _channelLoggingService;
        private readonly ILogger _logger;
        private NetworkParameters _networkParameters;
        
        public CommitmentTransactionService(ILoggerFactory loggerFactory, IKeyDerivationService derivationService, IChannelLoggingService channelLoggingService)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _derivationService = derivationService;
            _channelLoggingService = channelLoggingService;
        }
        
        public void Initialize(NetworkParameters networkParameters)
        {
            _networkParameters = networkParameters;
        }

        public void CreateInitialCommitmentTransactions(OpenChannelMessage openMessage, AcceptChannelMessage acceptMessage, LocalChannel channel, ECKeyPair revocationKey)
        {
            channel.LocalCommitmentTxParameters = CreateLocalCommitmentTxParameters(openMessage, acceptMessage, revocationKey);
            channel.RemoteCommitmentTxParameters = CreateRemoteCommitmentTxParameters(openMessage, acceptMessage);
            SignRemoteCommitmentTx(channel);
        }

        private void SignRemoteCommitmentTx(LocalChannel channel)
        {
            var builder = new CommitmentTransactionBuilder(channel, false, _networkParameters);

            Transaction rawTransaction = builder.Build();
            Key fundingPrivateKey = new Key(channel.LocalCommitmentTxParameters.FundingKey.PrivateKeyData);
            channel.RemoteCommitmentTxParameters.LocalSignature = builder.SignCommitmentTransaction(fundingPrivateKey, rawTransaction);
            _channelLoggingService.LogInfo(channel, "Remote Commitment Transaction", rawTransaction.ToString());
        }

        public ECKeyPair GetNextLocalPerCommitmentPoint(LocalChannel channel)
        {
            return _derivationService.DerivePerCommitmentPoint(channel.LocalCommitmentTxParameters.RevocationKey, channel.LocalCommitmentTxParameters.TransactionNumber + 1);
        }

        public void UpdateRemotePerCommitmentPoint(LocalChannel channel, ECKeyPair nextPerCommitmentPoint)
        {
            channel.RemoteCommitmentTxParameters.UpdatePerCommitmentPoint(nextPerCommitmentPoint);
        }

        public bool IsValidRemoteCommitmentSignature(LocalChannel channel, TransactionSignature signature)
        {
            var builder = new CommitmentTransactionBuilder(channel, true, _networkParameters);

            return builder.IsValidSignature(signature, channel.LocalCommitmentTxParameters.FundingKey);
        }
        
        private CommitmentTransactionParameters CreateLocalCommitmentTxParameters(OpenChannelMessage openMessage, AcceptChannelMessage acceptMessage, ECKeyPair revocationKey)
        {
            PublicKeyDerivation publicKeyDerivation = new PublicKeyDerivation(openMessage.FirstPerCommitmentPoint);
            RevocationPublicKeyDerivation revocationPublicKeyDerivation = new RevocationPublicKeyDerivation(openMessage.FirstPerCommitmentPoint);
            PublicKeyDerivation remotePublicKeyDerivation = new PublicKeyDerivation(acceptMessage.FirstPerCommitmentPoint);
            
            CommitmentTransactionParameters parameters = new CommitmentTransactionParameters
            {
                TransactionNumber = 0,
                RevocationKey = revocationKey,
                FundingKey = openMessage.FundingPubKey,
                HtlcBasepoint = openMessage.HtlcBasepoint,
                HtlcPublicKey = remotePublicKeyDerivation.Derive(openMessage.HtlcBasepoint),
                DelayedPaymentBasepoint = openMessage.DelayedPaymentBasepoint,
                DelayedPaymentPublicKey = publicKeyDerivation.Derive(openMessage.DelayedPaymentBasepoint),
                PaymentBasepoint = openMessage.PaymentBasepoint,
                PaymentPublicKey = remotePublicKeyDerivation.Derive(openMessage.PaymentBasepoint),
                RevocationBasepoint = openMessage.RevocationBasepoint,
                RevocationPublicKey = revocationPublicKeyDerivation.DerivePublicKey(acceptMessage.RevocationBasepoint),
                PerCommitmentKey = openMessage.FirstPerCommitmentPoint,
                ToLocalMsat = openMessage.FundingSatoshis * 1000 - openMessage.PushMSat,
                ToRemoteMsat = openMessage.PushMSat
            };
            
            return parameters;
        }

        private CommitmentTransactionParameters CreateRemoteCommitmentTxParameters(OpenChannelMessage openMessage, AcceptChannelMessage acceptMessage)
        {
            PublicKeyDerivation publicKeyDerivation = new PublicKeyDerivation(acceptMessage.FirstPerCommitmentPoint);
            RevocationPublicKeyDerivation revocationPublicKeyDerivation = new RevocationPublicKeyDerivation(acceptMessage.FirstPerCommitmentPoint);
            PublicKeyDerivation remotePublicKeyDerivation = new PublicKeyDerivation(openMessage.FirstPerCommitmentPoint);
            
            CommitmentTransactionParameters parameters = new CommitmentTransactionParameters
            {
                TransactionNumber = 0,
                HtlcBasepoint = acceptMessage.HtlcBasepoint,
                HtlcPublicKey = remotePublicKeyDerivation.Derive(acceptMessage.HtlcBasepoint),
                PaymentBasepoint = acceptMessage.PaymentBasepoint,
                PaymentPublicKey = remotePublicKeyDerivation.Derive(acceptMessage.PaymentBasepoint),
                DelayedPaymentBasepoint = acceptMessage.DelayedPaymentBasepoint,
                DelayedPaymentPublicKey = publicKeyDerivation.Derive(acceptMessage.DelayedPaymentBasepoint),
                RevocationBasepoint = acceptMessage.RevocationBasepoint,
                RevocationPublicKey = revocationPublicKeyDerivation.DerivePublicKey(openMessage.RevocationBasepoint),
                PerCommitmentKey = acceptMessage.FirstPerCommitmentPoint,
                FundingKey = acceptMessage.FundingPubKey,
                ToRemoteMsat = openMessage.FundingSatoshis * 1000 - openMessage.PushMSat,
                ToLocalMsat = openMessage.PushMSat
            };
            
            return parameters;
        }
    }
}