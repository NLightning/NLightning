using NBitcoin;
using NLightning.Cryptography;
using NLightning.Utils.Extensions;

namespace NLightning.Wallet.Commitment
{
    public class TransactionNumber
    {
        public static ulong CalculateObscured(ulong transactionNumber, ECKeyPair localPaymentBasepoint, ECKeyPair remotePaymentBasepoint)
        {
            // XOR the last 6 bytes of (SHA256(payment_basepoint from open_channel || payment_basepoint from accept_channel))

            var hashed = SHA256.ComputeHash(localPaymentBasepoint.PublicKeyCompressed
                .ConcatToNewArray(remotePaymentBasepoint.PublicKeyCompressed)).SubArray(26, 6);
            var numberData = transactionNumber.GetBytesBigEndian().SubArray(2, 6);
            
            var obscured = XOR(numberData, hashed);
            return new byte[] {0, 0}.ConcatToNewArray(obscured).ToULongBigEndian();
        }
        
        private static byte[] XOR(byte[] data1, byte[] data2)
        {
            for (int i = 0; i < data1.Length; i++)
            {
                data1[i] ^= data2[i];
            }
                
            return data1;
        }

        public static ulong CalculateSequence(ulong obscuredTxNumber)
        {
            return 0x80000000L | (obscuredTxNumber >> 24);
        }

        public static ulong CalculateLockTime(ulong obscuredTxNumber)
        {
            return (obscuredTxNumber & 0xffffffL) | 0x20000000;
        }

        public static ulong CalculateNumber(ulong sequence, ECKeyPair localPaymentBasepoint, ECKeyPair remotePaymentBasepoint)
        {
            return CalculateObscured(sequence, localPaymentBasepoint, remotePaymentBasepoint);
        }
    }
}