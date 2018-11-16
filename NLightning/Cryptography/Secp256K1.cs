using System;
using System.Linq;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace NLightning.Cryptography
{
    public class Secp256K1
    {
        public static readonly X9ECParameters CurveParams = ECNamedCurveTable.GetByName("secp256k1");
        public static readonly ECDomainParameters DomainParams = new ECDomainParameters(CurveParams.Curve, CurveParams.G, CurveParams.N, CurveParams.H, CurveParams.GetSeed());
        
        public static ECKeyPair GenerateKeyPair()
        {
            var secureRandom = new SecureRandom();
            var keyParams = new ECKeyGenerationParameters(DomainParams, secureRandom);
            var generator = new ECKeyPairGenerator("ECDSA");
            
            generator.Init(keyParams);
            var keyPair = generator.GenerateKeyPair();
            var privateKey = keyPair.Private as ECPrivateKeyParameters;
            
            return new ECKeyPair(privateKey.D.ToByteArrayUnsigned(), true);
        }
        
        public static bool VerifySignature(byte[] data, byte[] signature, ECKeyPair publicKey)
        {
            var signer = new ECDsaSigner();
            var params1 = new ECPublicKeyParameters(publicKey.PublicKeyParameters.Q, DomainParams);
            signer.Init(false, params1);
            return signer.VerifySignature(data, new BigInteger(1, signature.Take(32).ToArray()), new BigInteger(1, signature.Skip(32).ToArray()));
        }
        
    }
}