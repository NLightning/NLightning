using NLightning.Cryptography;
using NLightning.Utils.Extensions;
using Org.BouncyCastle.Math;

namespace NLightning.Wallet.KeyDerivation
{
    public class PrivateKeyDerivation
    {
        private readonly ECKeyPair _perCommitmentPoint;

        public PrivateKeyDerivation(ECKeyPair perCommitmentPoint)
        {
            _perCommitmentPoint = perCommitmentPoint;
        }

        public ECKeyPair Derive(ECKeyPair basePointSecret)
        {
            var hash = new BigInteger(1, HashPoints(_perCommitmentPoint.PublicKeyCompressed, basePointSecret.PublicKeyCompressed));
            return new ECKeyPair(basePointSecret.PrivateKey.D.Add(hash).ToByteArrayUnsigned(), true);
        }

        private static byte[] HashPoints(byte[] point1, byte[] point2)
        {
            return SHA256.ComputeHash(point1.ConcatToNewArray(point2));
        }
    }
}