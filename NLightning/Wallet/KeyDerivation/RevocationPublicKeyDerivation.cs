using NLightning.Cryptography;
using NLightning.Utils.Extensions;
using Org.BouncyCastle.Math;

namespace NLightning.Wallet.KeyDerivation
{
    public class RevocationPublicKeyDerivation
    {
        private readonly ECKeyPair _perCommitmentPoint;

        public RevocationPublicKeyDerivation(ECKeyPair perCommitmentPoint)
        {
            _perCommitmentPoint = perCommitmentPoint;
        }

        /*
         * revocationpubkey = revocation_basepoint * SHA256(revocation_basepoint || per_commitment_point) + per_commitment_point * SHA256(per_commitment_point || revocation_basepoint)
         *
         * part1 = revocation_basepoint * SHA256(revocation_basepoint || per_commitment_point)
         * part2 = per_commitment_point * SHA256(per_commitment_point || revocation_basepoint)
         */
        public ECKeyPair DerivePublicKey(ECKeyPair revocationBasePoint)
        {
            var hash1 = new BigInteger(1, HashPoints(revocationBasePoint.PublicKeyCompressed, _perCommitmentPoint.PublicKeyCompressed));
            var part1 = revocationBasePoint.PublicKeyParameters.Q.Multiply(hash1);
            
            var hash2 = new BigInteger(1, HashPoints(_perCommitmentPoint.PublicKeyCompressed, revocationBasePoint.PublicKeyCompressed));
            var part2 = _perCommitmentPoint.PublicKeyParameters.Q.Multiply(hash2);

            var result = part1.Add(part2);
            
            return new ECKeyPair(result.Normalize().GetEncoded(true), false);
        }

        private static byte[] HashPoints(byte[] point1, byte[] point2)
        {
            return SHA256.ComputeHash(point1.ConcatToNewArray(point2));
        }
    }
}