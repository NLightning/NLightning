using System.Collections.Generic;
using NBitcoin;
using NLightning.Cryptography;

namespace NLightning.Wallet
{
    public class MultiSignatureWitnessScript
    {
        public static WitScript Create(
            ECKeyPair pubKey1, ECKeyPair pubKey2,
            TransactionSignature pubKeySig1, TransactionSignature pubKeySig2)
        {
            byte[] pubKeySig1Data = pubKeySig1.ToBytes();
            byte[] pubKeySig2Data = pubKeySig2.ToBytes();
            
            var multiSigPubScript = MultiSignaturePubKey.GenerateMultisigPubKey(pubKey1, pubKey2);
            byte[] pubKeySignature1 = pubKey1 < pubKey2 ? pubKeySig1Data : pubKeySig2Data;
            byte[] pubKeySignature2 = pubKey1 < pubKey2 ? pubKeySig2Data : pubKeySig1Data;

            return new WitScript(new List<byte[]>
            {
                new byte[] {}, pubKeySignature1, pubKeySignature2, multiSigPubScript.ToBytes()
            });
        }
    }
}