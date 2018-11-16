using System;
using System.Collections.Generic;
using NBitcoin;
using NLightning.Network;
using NLightning.Peer.Channel.Models;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using NLightning.Wallet;

namespace NLightning.Peer.Channel
{
    public class CloseChannelTransactionBuilder
    {
        private readonly LocalChannel _channel;
        private readonly NetworkParameters _networkParameters;
        
        public CloseChannelTransactionBuilder(LocalChannel channel, NetworkParameters networkParameters)
        {
            _channel = channel;
            _networkParameters = networkParameters;
        }
        
        public ulong FeeSatoshi { get; set; }
        
        public Transaction Build()
        {
            ulong toLocalSatoshi = _channel.LocalCommitmentTxParameters.ToLocalMsat.MSatToSatoshi().CheckedSubtract(_channel.IsFunder ? FeeSatoshi : 0);
            ulong toRemoteSatoshi = _channel.LocalCommitmentTxParameters.ToRemoteMsat.MSatToSatoshi().CheckedSubtract(_channel.IsFunder ? 0 : FeeSatoshi);
            List<TxOut> outputs = new List<TxOut>();
            var dustLimitSatoshis = Math.Max(_channel.LocalChannelParameters.DustLimitSatoshis, _channel.RemoteChannelParameters.DustLimitSatoshis);
            
            if (toLocalSatoshi >= dustLimitSatoshis)
            {
                outputs.Add(BuildToLocalOutput(_channel, toLocalSatoshi));
            }
            
            if (toRemoteSatoshi >= dustLimitSatoshis)
            {
                outputs.Add(BuildToRemoteOutput(_channel, toRemoteSatoshi));
            }
           
            Transaction tx = Transaction.Create(_networkParameters.Network);
            
            TxIn txIn = new TxIn(new OutPoint(uint256.Parse(_channel.FundingTransactionId), _channel.FundingOutputIndex));
            txIn.Sequence = 0xFFFFFFFF;
            txIn.ScriptSig = MultiSignaturePubKey.GenerateMultisigPubKey(_channel.LocalCommitmentTxParameters.FundingKey, 
                _channel.RemoteCommitmentTxParameters.FundingKey).WitHash.ScriptPubKey;
            
            tx.Inputs.Add(txIn);
            tx.Outputs.AddRange(outputs);
            tx.Outputs.Sort(new Bip69OutputOrdering());
            tx.Version = 2;
            tx.LockTime = 0;
            
            return tx;   
        }

        public TransactionSignature Sign()
        {
            var unsigned = Build();
            var redeemScript = MultiSignaturePubKey.GenerateMultisigPubKey(_channel.LocalCommitmentTxParameters.FundingKey, 
                                                                            _channel.RemoteCommitmentTxParameters.FundingKey);
            var input = unsigned.Inputs[0];
            var amount = _channel.FundingSatoshis;
            var inputCoin = new Coin(input.PrevOut, new TxOut(Money.Satoshis(amount), input.ScriptSig));
            var scriptCoin = inputCoin.ToScriptCoin(redeemScript);
            Key fundingPrivateKey = new Key(_channel.LocalCommitmentTxParameters.FundingKey.PrivateKeyData);
            
            TransactionBuilder builder2 = new TransactionBuilder();
            builder2.AddCoins(scriptCoin);
            builder2.AddKeys(fundingPrivateKey);
            
            return unsigned.SignInput(fundingPrivateKey, scriptCoin);
        }
        
        private TxOut BuildToRemoteOutput(LocalChannel channel, ulong toRemoteSatoshi)
        {
            return new TxOut(toRemoteSatoshi, new Script(channel.RemoteChannelParameters.ShutdownScriptPubKey));
        }

        private TxOut BuildToLocalOutput(LocalChannel channel, ulong toLocalSatoshi)
        {
            return new TxOut(toLocalSatoshi, new Script(channel.LocalChannelParameters.ShutdownScriptPubKey));
        }
        
        public TransactionSignature SignClosingTransaction(Transaction unsignedClosingTx)
        {
            var redeemScript = MultiSignaturePubKey.GenerateMultisigPubKey(_channel.LocalCommitmentTxParameters.FundingKey,
                                                                            _channel.RemoteCommitmentTxParameters.FundingKey);
            var input = unsignedClosingTx.Inputs[0];
            var amount = _channel.FundingSatoshis;
            var inputCoin = new Coin(input.PrevOut, new TxOut(Money.Satoshis(amount), input.ScriptSig));
            var scriptCoin = inputCoin.ToScriptCoin(redeemScript);
            var fundingPrivateKey = new Key(_channel.LocalCommitmentTxParameters.FundingKey.PrivateKeyData);
            TransactionBuilder builder2 = new TransactionBuilder();
            builder2.AddCoins(scriptCoin);
            builder2.AddKeys(fundingPrivateKey);
            
            return unsignedClosingTx.SignInput(fundingPrivateKey, scriptCoin);
        }
        
        public Transaction BuildWithSignatures(TransactionSignature remotePubKeySignature)
        {
            var unsigned = Build();
            var localPubKeySignature = SignClosingTransaction(unsigned);
            var witScript = MultiSignatureWitnessScript.Create(
                _channel.LocalCommitmentTxParameters.FundingKey, _channel.RemoteCommitmentTxParameters.FundingKey,
                localPubKeySignature, remotePubKeySignature);

            unsigned.Inputs[0].ScriptSig = Script.Empty;
            unsigned.Inputs[0].WitScript = witScript;
            
            return unsigned;
        }
    }
}