using System.Collections.Generic;
using NBitcoin;
using NBitcoin.Crypto;

namespace NLightning.Wallet.Commitment
{
    public class OutputScripts
    {
        /*
         * https://github.com/lightningnetwork/lightning-rfc/blob/master/03-transactions.md#to_local-output
         *
         *   OP_IF
         *       # Penalty transaction
         *       <revocationpubkey>
         *   OP_ELSE
         *       `to_self_delay`
         *       OP_CSV
         *       OP_DROP
         *       <local_delayedpubkey>
         *   OP_ENDIF
         *   OP_CHECKSIG
         * 
         */
        public static Script ToLocal(PubKey localRevocationPubkey, PubKey localDelayedPaymentPubkey, ushort toSelfDelay)
        {
            List<Op> opList = new List<Op>();
            
            opList.Add(OpcodeType.OP_IF);
            opList.Add(Op.GetPushOp(localRevocationPubkey.ToBytes()));
            opList.Add(OpcodeType.OP_ELSE);
            opList.Add(Op.GetPushOp(toSelfDelay));
            opList.Add(OpcodeType.OP_CHECKSEQUENCEVERIFY);
            opList.Add(OpcodeType.OP_DROP);
            opList.Add(Op.GetPushOp(localDelayedPaymentPubkey.ToBytes()));
            opList.Add(OpcodeType.OP_ENDIF);
            opList.Add(OpcodeType.OP_CHECKSIG);
            
            return new Script(opList);
        }
        
        
        /*
         *  https://github.com/lightningnetwork/lightning-rfc/blob/master/03-transactions.md#offered-htlc-outputs
         * 
            OP_DUP OP_HASH160 <RIPEMD160(SHA256(revocationpubkey))> OP_EQUAL
            OP_IF
                OP_CHECKSIG
            OP_ELSE
                <remote_htlcpubkey> OP_SWAP OP_SIZE 32 OP_EQUAL
                OP_NOTIF
                    # To local node via HTLC-timeout transaction (timelocked).
                    OP_DROP 2 OP_SWAP <local_htlcpubkey> 2 OP_CHECKMULTISIG
                OP_ELSE
                    # To remote node with preimage.
                    OP_HASH160 <RIPEMD160(payment_hash)> OP_EQUALVERIFY
                    OP_CHECKSIG
                OP_ENDIF
            OP_ENDIF
         *
         */
        public static Script OfferedHtlc(PubKey localHtlcPubkey, PubKey remoteHtlcPubkey, PubKey localRevocationPubkey, byte[] paymentHash)
        {
            List<Op> opList = new List<Op>();
            
            opList.Add(OpcodeType.OP_DUP);
            opList.Add(OpcodeType.OP_HASH160);
            opList.Add(Op.GetPushOp(Hashes.Hash160(localRevocationPubkey.ToBytes()).ToBytes()));
            opList.Add(OpcodeType.OP_EQUAL);
            opList.Add(OpcodeType.OP_IF);
                opList.Add(OpcodeType.OP_CHECKSIG);
            opList.Add(OpcodeType.OP_ELSE);
                opList.Add(Op.GetPushOp(remoteHtlcPubkey.ToBytes()));
                opList.Add(OpcodeType.OP_SWAP);
                opList.Add(OpcodeType.OP_SIZE);
                opList.Add(Op.GetPushOp(32));
                opList.Add(OpcodeType.OP_EQUAL);
                opList.Add(OpcodeType.OP_NOTIF);
                    opList.Add(OpcodeType.OP_DROP);
                    opList.Add(OpcodeType.OP_2);
                    opList.Add(OpcodeType.OP_SWAP);
                    opList.Add(Op.GetPushOp(localHtlcPubkey.ToBytes()));
                    opList.Add(OpcodeType.OP_2);
                    opList.Add(OpcodeType.OP_CHECKMULTISIG);
                opList.Add(OpcodeType.OP_ELSE);
                    opList.Add(OpcodeType.OP_HASH160);
                    opList.Add(Op.GetPushOp(Hashes.RIPEMD160(paymentHash, paymentHash.Length)));
                    opList.Add(OpcodeType.OP_EQUALVERIFY);
                    opList.Add(OpcodeType.OP_CHECKSIG);
                opList.Add(OpcodeType.OP_ENDIF);
            opList.Add(OpcodeType.OP_ENDIF);
            return new Script(opList);
        }
        
        /*
         * https://github.com/lightningnetwork/lightning-rfc/blob/master/03-transactions.md#received-htlc-outputs

            # To remote node with revocation key
            OP_DUP OP_HASH160 <RIPEMD160(SHA256(revocationpubkey))> OP_EQUAL
            OP_IF
                OP_CHECKSIG
            OP_ELSE
                <remote_htlcpubkey> OP_SWAP
                    OP_SIZE 32 OP_EQUAL
                OP_IF
                    # To local node via HTLC-success transaction.
                    OP_HASH160 <RIPEMD160(payment_hash)> OP_EQUALVERIFY
                    2 OP_SWAP <local_htlcpubkey> 2 OP_CHECKMULTISIG
                OP_ELSE
                    # To remote node after timeout.
                    OP_DROP <cltv_expiry> OP_CHECKLOCKTIMEVERIFY OP_DROP
                    OP_CHECKSIG
                OP_ENDIF
            OP_ENDIF
                      
         */
        public static Script ReceivedHtlc(PubKey localHtlcPubkey, PubKey remoteHtlcPubkey, PubKey localRevocationPubkey,
            byte[] paymentHash, uint expiry)
        {
            List<Op> opList = new List<Op>();
            
            opList.Add(OpcodeType.OP_DUP);
            opList.Add(OpcodeType.OP_HASH160);
            opList.Add(Op.GetPushOp(Hashes.Hash160(localRevocationPubkey.ToBytes()).ToBytes()));
            opList.Add(OpcodeType.OP_EQUAL);
            opList.Add(OpcodeType.OP_IF);
                opList.Add(OpcodeType.OP_CHECKSIG);
            opList.Add(OpcodeType.OP_ELSE);
                opList.Add(Op.GetPushOp(remoteHtlcPubkey.ToBytes()));
                opList.Add(OpcodeType.OP_SWAP);
                opList.Add(OpcodeType.OP_SIZE);
                opList.Add(Op.GetPushOp(32));
                opList.Add(OpcodeType.OP_EQUAL);
                opList.Add(OpcodeType.OP_IF);
                    opList.Add(OpcodeType.OP_HASH160);
                    opList.Add(Op.GetPushOp(Hashes.RIPEMD160(paymentHash, paymentHash.Length)));
                    opList.Add(OpcodeType.OP_EQUALVERIFY);
                    opList.Add(OpcodeType.OP_2);
                    opList.Add(OpcodeType.OP_SWAP);
                    opList.Add(Op.GetPushOp(localHtlcPubkey.ToBytes()));
                    opList.Add(OpcodeType.OP_2);
                    opList.Add(OpcodeType.OP_CHECKMULTISIG);
                opList.Add(OpcodeType.OP_ELSE);
                    opList.Add(OpcodeType.OP_DROP);
                    opList.Add(Op.GetPushOp(expiry));
                    opList.Add(OpcodeType.OP_CHECKLOCKTIMEVERIFY);
                    opList.Add(OpcodeType.OP_DROP);
                    opList.Add(OpcodeType.OP_CHECKSIG);
                opList.Add(OpcodeType.OP_ENDIF);
            opList.Add(OpcodeType.OP_ENDIF);
            return new Script(opList);
        }
    }
}