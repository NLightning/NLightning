using NBitcoin;
using NLightning.Cryptography;

namespace NLightning.Wallet
{
    public class MultiSignaturePubKey
    {
        public static Script GenerateMultisigPubKey(ECKeyPair pubKey1, ECKeyPair pubKey2)
        {
            ECKeyPair pubKeySorted1 = pubKey1 > pubKey2 ? pubKey2 : pubKey1;
            ECKeyPair pubKeySorted2 = pubKey1 > pubKey2 ? pubKey1 : pubKey2;
            return PayToMultiSigTemplate
                .Instance
                .GenerateScriptPubKey(2, new PubKey(pubKeySorted1.PublicKeyCompressed), new PubKey(pubKeySorted2.PublicKeyCompressed));
        }
    }
}