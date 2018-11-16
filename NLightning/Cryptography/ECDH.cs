using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC;

namespace NLightning.Cryptography
{
    public class ECDH
    {
        public static byte[] ComputeHashedPoint(ECPublicKeyParameters publicKey, ECPrivateKeyParameters privateKey)
        {
            ECPoint point = publicKey.Q.Multiply(privateKey.D).Normalize();
            return SHA256.ComputeHash(point.GetEncoded(true));
        }
    }
}