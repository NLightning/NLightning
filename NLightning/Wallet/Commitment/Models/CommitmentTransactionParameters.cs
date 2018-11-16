using NBitcoin;
using NLightning.Cryptography;
using NLightning.Wallet.KeyDerivation;

namespace NLightning.Wallet.Commitment.Models
{
    public class CommitmentTransactionParameters
    {
        public int Id { get; set; }
        public uint TransactionNumber { get; set; }
        public ECKeyPair HtlcBasepoint { get; set; }
        public ECKeyPair PaymentBasepoint { get; set; }
        public ECKeyPair RevocationBasepoint { get; set; }
        public ECKeyPair DelayedPaymentBasepoint { get; set; }
        public ECKeyPair RevocationKey { get; set; }
        public ECKeyPair FundingKey { get; set; }
        
        public ECKeyPair HtlcPublicKey { get; set; }
        public ECKeyPair PaymentPublicKey { get; set; }
        public ECKeyPair RevocationPublicKey { get; set; }
        public ECKeyPair DelayedPaymentPublicKey { get; set; }
        
        public ECKeyPair PerCommitmentKey { get; set; }
        public ECKeyPair NextPerCommitmentPoint { get; set; }
        
        public ulong ToLocalMsat { get; set; }
        public ulong ToRemoteMsat { get; set; }
        public TransactionSignature LocalSignature { get; set; }
        public TransactionSignature RemoteSignature { get; set; }

        public void UpdatePerCommitmentPoint(ECKeyPair commitmentPoint)
        {
            PublicKeyDerivation publicKeyDerivation = new PublicKeyDerivation(commitmentPoint);
            HtlcPublicKey = publicKeyDerivation.Derive(HtlcBasepoint);
            PaymentPublicKey = publicKeyDerivation.Derive(PaymentBasepoint);
            DelayedPaymentPublicKey = publicKeyDerivation.Derive(DelayedPaymentBasepoint);
            PerCommitmentKey = commitmentPoint;
        }
    }
}