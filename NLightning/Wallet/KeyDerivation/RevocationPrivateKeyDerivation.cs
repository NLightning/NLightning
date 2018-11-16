using NLightning.Cryptography;
using NLightning.Utils.Extensions;
using Org.BouncyCastle.Math;

namespace NLightning.Wallet.KeyDerivation
{
    public class RevocationPrivateKeyDerivation
    {
        private readonly ECKeyPair _perCommitmentSecret;

        public RevocationPrivateKeyDerivation(ECKeyPair perCommitmentSecret)
        {
            _perCommitmentSecret = perCommitmentSecret;
        }
        
        /*
         * revocationprivkey = (revocation_basepoint_secret * SHA256(revocation_basepoint || per_commitment_point) + per_commitment_secret * SHA256(per_commitment_point || revocation_basepoint)) mod N
         *
         */
        public ECKeyPair DerivePrivateKey(ECKeyPair basePointSecret)
        {
            var hash1 = new BigInteger(1, HashPoints(basePointSecret.PublicKeyCompressed, _perCommitmentSecret.PublicKeyCompressed));
            var hash2 = new BigInteger(1, HashPoints(_perCommitmentSecret.PublicKeyCompressed, basePointSecret.PublicKeyCompressed));
            
            var part1 = basePointSecret.PrivateKey.D.Multiply(hash1);
            var part2 = _perCommitmentSecret.PrivateKey.D.Multiply(hash2);
            
            var part1AndPart2 = part1.Add(part2);
            var revocationPrivate = part1AndPart2.Mod(basePointSecret.PrivateKey.Parameters.N);
            
            return new ECKeyPair(revocationPrivate.ToByteArrayUnsigned(), true);
        }

        private static byte[] HashPoints(byte[] point1, byte[] point2)
        {
            return SHA256.ComputeHash(point1.ConcatToNewArray(point2));
        }
    }
}