using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.Peer.Channel.Models;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using NLightning.Wallet.Commitment.Models;

namespace NLightning.Wallet.Commitment
{
    /*
     * https://github.com/lightningnetwork/lightning-rfc/blob/master/03-transactions.md#commitment-transaction-construction
     */
    public class CommitmentTransactionBuilder
    {
        private Dictionary<Htlc, int> _htlcOutputIndexMap = new Dictionary<Htlc, int>();
        private Transaction _commitmentTransaction;
        private LocalChannel _channel;
        
        public NBitcoin.Network Network { get; set; }
        public bool IsFunder { get; set; }

        public List<Htlc> Htlcs { get; set; } = new List<Htlc>();
        public ChannelParameters ChannelParameters { get; set; }
        public CommitmentTransactionParameters CommitmentTxParams { get; set; }
        
        public ECKeyPair RemotePaymentPubKey { get; set; }
        public ECKeyPair RemoteHtlcPubkey { get; set; }
        public ECKeyPair RemoteFundingPubKey { get; set; }
        public ECKeyPair RemotePaymentBasePoint { get; set; }
        
        public ulong FeeratePerKw { get; set; }
        public ulong FundingAmount { get; set; }
        public string FundingTransactionHash { get; set; }
        public ushort FundingTransactionOutputIndex { get; set; }
        
        public LocalChannel Channel
        {
            set
            {
                _channel = value;
                FundingAmount = _channel.FundingSatoshis;
                FundingTransactionHash = _channel.FundingTransactionId;
                FundingTransactionOutputIndex = _channel.FundingOutputIndex;
                FeeratePerKw = _channel.FeeRatePerKw;
                Htlcs = _channel.Htlcs;
            }
        }

        public CommitmentTransactionParameters RemoteCommitmentTxParams
        {
            set
            {
                RemoteHtlcPubkey = value.HtlcPublicKey;
                RemotePaymentPubKey = value.PaymentPublicKey;
                RemoteFundingPubKey = value.FundingKey;
                RemotePaymentBasePoint = value.PaymentBasepoint;
            }
        }

        public CommitmentTransactionBuilder()
        {
        }
        
        public CommitmentTransactionBuilder(LocalChannel channel, bool buildLocal, NetworkParameters networkParameters)
        {
            Network = networkParameters.Network;
            Channel = channel;

            if (buildLocal)
            {
                IsFunder = channel.IsFunder;
                ChannelParameters = channel.LocalChannelParameters;
                CommitmentTxParams = channel.LocalCommitmentTxParameters;
                RemoteCommitmentTxParams = channel.RemoteCommitmentTxParameters;
            }
            else
            {
                IsFunder = !channel.IsFunder;
                ChannelParameters = channel.RemoteChannelParameters;
                CommitmentTxParams = channel.RemoteCommitmentTxParameters;
                RemoteCommitmentTxParams = channel.LocalCommitmentTxParameters;
            }
        }

        public Transaction Build()
        {
            var trimmedOfferedHtlcs = GetTrimmedOfferedHtlcs();
            var trimmedReceivedHtlcs = GetTrimmedReceivedHtlcs();
            ulong feeSatoshi = CalculateFee(trimmedOfferedHtlcs, trimmedReceivedHtlcs);
            ulong toLocalSatoshi = CommitmentTxParams.ToLocalMsat.MSatToSatoshi().CheckedSubtract(IsFunder ? feeSatoshi : 0);
            ulong toRemoteSatoshi = CommitmentTxParams.ToRemoteMsat.MSatToSatoshi().CheckedSubtract(IsFunder ? 0 : feeSatoshi);
            List<(Htlc, TxOut)> htlcTxOutMap = new List<(Htlc, TxOut)>();
            List<TxOut> outputs = new List<TxOut>();
                 
            if (toRemoteSatoshi >= ChannelParameters.DustLimitSatoshis)
            {
                outputs.Add(BuildToRemoteOutput(toRemoteSatoshi));
            }
            
            if (toLocalSatoshi >= ChannelParameters.DustLimitSatoshis)
            {
                outputs.Add(BuildToLocalDelayedOutput(toLocalSatoshi));
            }

            outputs.AddRange(trimmedOfferedHtlcs.Select(htlc =>
            {
                var txOut = BuildOfferedHtlcOutput(htlc);
                htlcTxOutMap.Add((htlc, txOut));
                return txOut;
            }));
            
            outputs.AddRange(trimmedReceivedHtlcs.Select(htlc =>
            {
                var txOut = BuildReceivedHtlcOutput(htlc);
                htlcTxOutMap.Add((htlc, txOut));
                return txOut;
            }));

            ulong obscuredTxNumber = IsFunder
                ? TransactionNumber.CalculateObscured(CommitmentTxParams.TransactionNumber, CommitmentTxParams.PaymentBasepoint, RemotePaymentBasePoint)
                : TransactionNumber.CalculateObscured(CommitmentTxParams.TransactionNumber, RemotePaymentBasePoint, CommitmentTxParams.PaymentBasepoint);
            ulong sequence = TransactionNumber.CalculateSequence(obscuredTxNumber);
            ulong lockTime = TransactionNumber.CalculateLockTime(obscuredTxNumber);
            
            Transaction tx = Transaction.Create(Network);
            TxIn txIn = new TxIn(new OutPoint(uint256.Parse(FundingTransactionHash), FundingTransactionOutputIndex));
            txIn.Sequence = new Sequence((uint)sequence);
            txIn.ScriptSig = MultiSignaturePubKey.GenerateMultisigPubKey(CommitmentTxParams.FundingKey, RemoteFundingPubKey).WitHash.ScriptPubKey;
            
            tx.Inputs.Add(txIn);
            tx.Outputs.AddRange(outputs);
            tx.LockTime = new LockTime((uint)lockTime);
            tx.Version = 2;

            tx.Outputs.Sort(new Bip69OutputOrdering());
            _htlcOutputIndexMap = htlcTxOutMap.ToDictionary(k => k.Item1, v =>  tx.Outputs.FindIndex(o => o == v.Item2));
            
            return _commitmentTransaction = tx;
        }
      
        public TransactionSignature SignCommitmentTransaction(Key fundingPrivateKey, Transaction unsignedCommitmentTransaction = null)
        {
            var unsigned = unsignedCommitmentTransaction ?? Build();
            var redeemScript = MultiSignaturePubKey.GenerateMultisigPubKey(CommitmentTxParams.FundingKey, RemoteFundingPubKey);
            var input = unsigned.Inputs[0];
            var inputCoin = new Coin(input.PrevOut, new TxOut(Money.Satoshis(FundingAmount), input.ScriptSig));
            var scriptCoin = inputCoin.ToScriptCoin(redeemScript);
            
            TransactionBuilder builder2 = new TransactionBuilder();
            builder2.AddCoins(scriptCoin);
            builder2.AddKeys(fundingPrivateKey);
            
            return unsigned.SignInput(fundingPrivateKey, scriptCoin);
        }

        public Transaction BuildWithSignatures()
        {
            var key = new Key(CommitmentTxParams.FundingKey.PrivateKeyData);
            return BuildWithSignatures(CommitmentTxParams.RemoteSignature, key);
        }

        public Transaction BuildWithSignatures(TransactionSignature remotePubKeySignature, Key fundingPrivateKey)
        {
            var unsigned = Build();
            var localPubKeySignature = SignCommitmentTransaction(fundingPrivateKey, unsigned);
            var witScript = MultiSignatureWitnessScript.Create(
                CommitmentTxParams.FundingKey, RemoteFundingPubKey,
                localPubKeySignature, remotePubKeySignature);

            unsigned.Inputs[0].ScriptSig = Script.Empty;
            unsigned.Inputs[0].WitScript = witScript;
            
            return unsigned;
        }
        
        private TxOut BuildOfferedHtlcOutput(Htlc trimmedOfferedHtlc)
        {
            PubKey localHtlcPubkey = new PubKey(CommitmentTxParams.HtlcPublicKey.PublicKeyCompressed);
            PubKey remoteHtlcPubkey = new PubKey(RemoteHtlcPubkey.PublicKeyCompressed);
            PubKey localRevocationPubkey = new PubKey(CommitmentTxParams.RevocationPublicKey.PublicKeyCompressed);
            Script script = OutputScripts.OfferedHtlc(localHtlcPubkey, remoteHtlcPubkey, localRevocationPubkey, trimmedOfferedHtlc.PaymentHash);
            
            return new TxOut(Money.Satoshis(trimmedOfferedHtlc.AmountMsat.MSatToSatoshi()), script.WitHash.ScriptPubKey);  
        }
        
        private TxOut BuildReceivedHtlcOutput(Htlc trimmedReceivedHtlc)
        {
            PubKey localHtlcPubkey = new PubKey(CommitmentTxParams.HtlcPublicKey.PublicKeyCompressed);
            PubKey remoteHtlcPubkey = new PubKey(RemoteHtlcPubkey.PublicKeyCompressed);
            PubKey localRevocationPubkey = new PubKey(CommitmentTxParams.RevocationPublicKey.PublicKeyCompressed);
            Script script = OutputScripts.ReceivedHtlc(localHtlcPubkey, remoteHtlcPubkey, localRevocationPubkey, trimmedReceivedHtlc.PaymentHash, trimmedReceivedHtlc.Expiry);
            
            return new TxOut(Money.Satoshis(trimmedReceivedHtlc.AmountMsat.MSatToSatoshi()), script.WitHash.ScriptPubKey);  
        }

        private TxOut BuildToRemoteOutput(ulong toRemote)
        {
            PubKey pubKey = new PubKey(RemotePaymentPubKey.PublicKeyCompressed);
            return new TxOut(Money.Satoshis(toRemote), pubKey.WitHash.ScriptPubKey);   
        }

        private TxOut BuildToLocalDelayedOutput(ulong toLocal)
        {
            PubKey localRevocationPubkey = new PubKey(CommitmentTxParams.RevocationPublicKey.PublicKeyCompressed);
            PubKey localDelayedPaymentPubkey = new PubKey(CommitmentTxParams.DelayedPaymentPublicKey.PublicKeyCompressed);
            Script script = OutputScripts.ToLocal(localRevocationPubkey, localDelayedPaymentPubkey, ChannelParameters.ToSelfDelay);
            return new TxOut(Money.Satoshis(toLocal), script.WitHash.ScriptPubKey);   
        }

        private ulong CalculateFee(List<Htlc> trimmedOfferedHtlcs, List<Htlc> trimmedReceivedHtlcs)
        {
            var weight = TransactionFee.CommitWeight + 172 * ((ulong)trimmedOfferedHtlcs.Count + (ulong)trimmedReceivedHtlcs.Count);
            return TransactionFee.CalculateFee(FeeratePerKw, weight);
        }

        private List<Htlc> GetTrimmedOfferedHtlcs()
        {
            var htlcTimeoutFee = TransactionFee.CalculateFee(FeeratePerKw, TransactionFee.HtlcTimeoutWeight);
            return Htlcs.Where(h => h.Direction == Direction.Outgoing &&
                                    h.AmountMsat >= (ChannelParameters.DustLimitSatoshis + htlcTimeoutFee)*1000).ToList();
        }
        
        private List<Htlc> GetTrimmedReceivedHtlcs()
        {
            var htlcSuccessFee = TransactionFee.CalculateFee(FeeratePerKw, TransactionFee.HtlcSuccessWeight);
            return Htlcs.Where(h => h.Direction == Direction.Incoming &&
                                    h.AmountMsat >= (ChannelParameters.DustLimitSatoshis + htlcSuccessFee)*1000).ToList();
        }

        public List<Transaction> BuildHtlcTimeoutTransactions()
        {
            return GetTrimmedOfferedHtlcs().Select(BuildHtlcTransaction).ToList();
        }

        public List<Transaction> BuildHtlcSuccessTransactions()
        {
            return GetTrimmedReceivedHtlcs().Select(BuildHtlcTransaction).ToList();
        }
        
        private Transaction BuildHtlcTransaction(Htlc htlc)
        {
            bool incoming = htlc.Direction == Direction.Incoming;
            var fee = TransactionFee.CalculateFee(FeeratePerKw, incoming ? TransactionFee.HtlcSuccessWeight : TransactionFee.HtlcTimeoutWeight);
            var witnessScript = CreateHtlcWitnessScript(htlc);
            var outputScript = OutputScripts.ToLocal(CommitmentTxParams.RevocationPublicKey.ToPubKey(), CommitmentTxParams.DelayedPaymentPublicKey.ToPubKey(), ChannelParameters.ToSelfDelay);
            var outputIndex = _htlcOutputIndexMap[htlc];
            var amount = htlc.AmountMsat.MSatToSatoshi() - fee;
            if (amount < ChannelParameters.DustLimitSatoshis) {
                throw new InvalidOperationException("HTLC amount below dust limit");
            }
                
            Transaction tx = Transaction.Create(Network);
            TxIn txIn = new TxIn(new OutPoint(_commitmentTransaction, outputIndex));
            txIn.Sequence = 0;
            txIn.ScriptSig = witnessScript.WitHash.ScriptPubKey;
            tx.Inputs.Add(txIn);
                
            TxOut txOut = new TxOut(Money.Satoshis(amount), outputScript.WitHash.ScriptPubKey);
            tx.Outputs.Add(txOut);
                
            tx.Version = 2;
            tx.LockTime = incoming ? 0 : htlc.Expiry;
    
            return tx;   
        }

        private Script CreateHtlcWitnessScript(Htlc htlc)
        {
            return htlc.Direction == Direction.Incoming ?
                OutputScripts.ReceivedHtlc(CommitmentTxParams.HtlcPublicKey.ToPubKey(),
                        RemoteHtlcPubkey.ToPubKey(),
                        CommitmentTxParams.RevocationPublicKey.ToPubKey(), htlc.PaymentHash, htlc.Expiry) :
                OutputScripts.OfferedHtlc(CommitmentTxParams.HtlcPublicKey.ToPubKey(),
                        RemoteHtlcPubkey.ToPubKey(),
                        CommitmentTxParams.RevocationPublicKey.ToPubKey(), htlc.PaymentHash);
        }

        public Transaction SignHtlcTransaction(Htlc htlc, Transaction transaction, byte[] htlcRemoteSignature, Key fundingPrivateKey)
        {
            var witnessScript = CreateHtlcWitnessScript(htlc);
            var input = transaction.Inputs[0];
            var outputIndex = _htlcOutputIndexMap[htlc];
            var amount = _commitmentTransaction.Outputs[outputIndex].Value;
            var inputCoin = new Coin(input.PrevOut, new TxOut(Money.Satoshis(amount), input.ScriptSig));
            var scriptCoin = inputCoin.ToScriptCoin(witnessScript);
            
            TransactionBuilder builder2 = new TransactionBuilder();
            builder2.AddCoins(scriptCoin);
            builder2.AddKeys(fundingPrivateKey);
            
            var signature = transaction.SignInput(fundingPrivateKey, scriptCoin);
            var witScript = CreateHtlcWitnessScript(htlc, signature.ToBytes(), htlcRemoteSignature, witnessScript);

            transaction.Inputs[0].ScriptSig = Script.Empty;
            transaction.Inputs[0].WitScript = witScript;
            
            return transaction;
        }
        
        private WitScript CreateHtlcWitnessScript(Htlc htlc, byte[] localPubKeySignature, byte[] remotePubKeySignature,
            Script witnessScript)
        {
            bool incoming = htlc.Direction == Direction.Incoming;
            return new WitScript(new List<byte[]>()
                {new byte[] {0}, remotePubKeySignature, localPubKeySignature, incoming ? htlc.PaymentPreImage : new byte[] {0}, witnessScript.ToBytes()});
        }

        public bool IsValidSignature(TransactionSignature remoteSignature, ECKeyPair fundingPrivateKey)
        {
            var privateKey = new Key(fundingPrivateKey.PrivateKeyData);
            var txWithSignatures = BuildWithSignatures(remoteSignature, privateKey);
            var unsigned = Build();
            var input = unsigned.Inputs[0];

            TransactionBuilder tb = new TransactionBuilder();
            var fundingCoins = new Coin(uint256.Parse(FundingTransactionHash), FundingTransactionOutputIndex, Money.Satoshis(FundingAmount), input.ScriptSig);
            tb.AddCoins(fundingCoins);
            var policyErrors = tb.Check(txWithSignatures);

            return !policyErrors.Any();
        }
    }
}