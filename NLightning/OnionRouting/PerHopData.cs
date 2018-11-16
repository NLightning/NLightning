using System;
using NLightning.Cryptography;
using NLightning.Utils.Extensions;

namespace NLightning.OnionRouting
{
    public class PerHopData
    {
        public PerHopData(ECKeyPair publicKey, byte[] shortChannelId, ulong amountToForward, 
                          uint outgoingCltvValue, byte[] padding, byte realm = 0x0)
        {
            ShortChannelId = shortChannelId;
            AmountToForward = amountToForward;
            OutgoingCltvValue = outgoingCltvValue;
            Padding = padding;
            PublicKey = publicKey;
            Realm = realm;
        }

        public ECKeyPair PublicKey { get; }
        public byte Realm { get; }
        public byte[] ShortChannelId { get; }
        public ulong AmountToForward { get; }
        public uint OutgoingCltvValue { get; }
        public byte[] Padding { get; }
        
        public byte[] GetDataWithRealmAndHmac(byte[] nextHmac)
        {
            if (ShortChannelId.Length != 8)
            {
                throw new ArgumentException("invalid property size.", nameof(ShortChannelId));
            }
            
            if (Padding.Length != 12)
            {
                throw new ArgumentException("invalid property size.", nameof(Padding));
            }
            
            if (nextHmac.Length != 32)
            {
                throw new ArgumentException("invalid property size.", nameof(nextHmac));
            }
            
            return ByteExtensions.Combine(
                new[] { Realm }, 
                ShortChannelId, 
                AmountToForward.GetBytesBigEndian(),
                OutgoingCltvValue.GetBytesBigEndian(), 
                Padding, 
                nextHmac);
        }
    }
}