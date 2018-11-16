using System;
using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Utils.Extensions;
using Org.BouncyCastle.Math;

namespace NLightning.OnionRouting
{
    public class OnionPacketBuilder
    {
        private const ushort HopsMaximumCount = 20;
        private const ushort HopDataSize = 65;
        private const ushort HmacSize = 32;
        private static readonly byte[] Rho = { 0x72, 0x68, 0x6F };
        private static readonly byte[] Mu = { 0x6d, 0x75 };
        private static readonly byte[] Nonce = new byte[12];
        
        public OnionPacketBuilder(List<PerHopData> nodePath)
        {
            HopsData = nodePath;
        }
        
        public ECKeyPair SessionKey { get; set; }
        public byte[] AssociatedData { get; set; }
        public List<PerHopData> HopsData { get; set; }
        
        /*
         *  Implementation of
         *  https://github.com/lightningnetwork/lightning-rfc/blob/master/04-onion-routing.md#packet-construction
         */
        public OnionPacket Build()
        {
            var hopsCount = HopsData.Count;
            List<byte[]> hopSharedSecrets = new List<byte[]>();

            var ephemeralKey = SessionKey.PrivateKey.D;
            var ephemeralKeys = new List<BigInteger>();
            for (var i = 0; i < hopsCount; i++)
            {
                ephemeralKeys.Add(ephemeralKey);
                
                // Perform ECDH and hash the result.
                var ecdhResult = HopsData[i].PublicKey.PublicKeyParameters.Q.Multiply(ephemeralKey);
                var hopSharedSecret = SHA256.ComputeHash(ecdhResult.GetEncoded());
                hopSharedSecrets.Add(hopSharedSecret);
                
                // Derive ephemeral public key from private key.
                ECKeyPair ephemeralKeyPair = new ECKeyPair(ephemeralKey.ToByteArrayUnsigned(), true);

                // Compute blinding factor.
                var combined = ephemeralKeyPair.PublicKeyCompressed.ConcatToNewArray(hopSharedSecret);
          
                BigInteger blindingFactor = new BigInteger(1, SHA256.ComputeHash(combined));
                
                // Blind ephemeral key for next hop.
                ephemeralKey = ephemeralKey.Multiply(blindingFactor);
                ephemeralKey = ephemeralKey.Mod(ECKeyPair.Secp256K1.N);
            }

            // Generate the padding, called "filler strings" in the paper.
            var filler = GenerateFiller(Rho, hopsCount, HopDataSize, hopSharedSecrets);

            // Allocate and initialize fields to zero-filled slices
            var routingInfoSize = HopDataSize * HopsMaximumCount;
            var routingInfo = new byte[routingInfoSize];
            var nextHmac = new byte[HmacSize];

            // Compute the routing information for each hop along with a
            // MAC of the routing information using the shared key for that hop.
            for (int i = hopsCount - 1; i >= 0; i--)
            {
                var rhoKey = GenerateKey(Rho, hopSharedSecrets[i]);
                var muKey = GenerateKey(Mu, hopSharedSecrets[i]);    

                var perHopData = HopsData[i].GetDataWithRealmAndHmac(nextHmac);
                var streamBytes = GenerateCipherStream(rhoKey, (uint)routingInfoSize);
                
                // Shift right
                Array.Copy(routingInfo, 0, routingInfo, HopDataSize, routingInfo.Length - HopDataSize);
                // Add per hop data
                Array.Copy(perHopData, 0, routingInfo, 0, perHopData.Length);

                routingInfo = Xor(routingInfo, streamBytes);
                
                if (i == hopsCount - 1)
                {
                    Array.Copy(filler, 0, routingInfo, routingInfo.Length - filler.Length, filler.Length);
                }

                var packet = routingInfo.ConcatToNewArray(AssociatedData);
                nextHmac = CalculateMac(muKey, packet);    
            }

            byte version = 0x0;
            var data = ByteExtensions.Combine(new byte[] {version}, SessionKey.PublicKeyCompressed, routingInfo, nextHmac);
            return new OnionPacket(SessionKey, nextHmac, HopsData, version, data);
        }

        private byte[] GenerateFiller(byte[] key, int hopsCount, uint hopSize, List<byte[]> sharedSecrets)
        {
            uint fillerSize = (HopsMaximumCount + 1) * hopSize;
            byte[] filler = new byte[fillerSize];
            byte[] zeroHop = new byte[hopSize];
            
            // The last hop does not obfuscate, it's not forwarding anymore.
            for (var i = 0; i < hopsCount -1; i++) {

                // Left-shift the field
                Array.Copy(filler, hopSize, filler, 0, fillerSize - hopSize);
                    
                // Zero-fill the last hop
                Array.Copy(zeroHop, 0, filler, fillerSize-hopSize, hopSize);

                // Generate pseudo-random byte stream
                var streamKey = GenerateKey(key, sharedSecrets[i]);
                var streamBytes = GenerateCipherStream(streamKey, fillerSize);

                // Obfuscate
                filler = Xor(filler, streamBytes);
            }
            
            // Cut filler down to the correct length (numHops+1)*hopSize
            // bytes will be prepended by the packet generation.
            int cutoff = (int) ((HopsMaximumCount - hopsCount + 2) * hopSize);
            return filler.SubArray(cutoff, filler.Length - cutoff);
        }

        private byte[] GenerateKey(byte[] key, byte[] sharedSecret)
        {
            return HmacSha256.ComputeHash(key, sharedSecret);
        }
        
        private byte[] GenerateCipherStream(byte[] key, uint fillerSize)
        {
            byte[] zeroData = new byte[fillerSize];
            var cipher = ChaCha20Poly1305.ChaCha20Encrypt(zeroData, key, Nonce);
            return cipher;
        }

        private byte[] CalculateMac(byte[] key, byte[] data)
        {
            return HmacSha256.ComputeHash(key, data);
        }

        private byte[] Xor(byte[] target, byte[] xorData)
        {
            if (xorData.Length > target.Length)
            {
                throw new ArgumentException($"{nameof(target)}.Length needs to be >= {nameof(xorData)}.Length");
            }
            
            for (int i = 0; i < xorData.Length; i++)
            {
                target[i] = (byte)(target[i] ^ xorData[i]);
            }

            return target;
        }
    }
}