using NLightning.Cryptography;
using NLightning.Utils.Extensions;
using NLightning.Wallet.Commitment;
using Xunit;

namespace NLightning.Test.Wallet.Commitment
{
    public class TransactionNumberTests
    {
        /*
         * Test Vectors:
         * https://github.com/lightningnetwork/lightning-rfc/blob/master/03-transactions.md#appendix-c-commitment-and-htlc-transaction-test-vectors
         */
        [Fact]
        public void CalculateObscuredTest()
        {
            var localPaymentBasePoint =
                new ECKeyPair("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa", false);
            var remotePaymentBasePoint =
                new ECKeyPair("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991", false);

            ulong actual = TransactionNumber.CalculateObscured(42, localPaymentBasePoint, remotePaymentBasePoint);

            var expected = 0x2bb038521914L ^ 42L;
            Assert.Equal((ulong)expected, actual);
        }

        [Fact]
        public void CalculateSequenceTest()
        {
            ulong obscured = 0x2bb038521914L ^ 42L;
            
            Assert.Equal((ulong)2150346808, TransactionNumber.CalculateSequence(obscured));
        }
       
        [Fact]
        public void CalculateLockTimeTest()
        {
            ulong obscured = 0x2bb038521914L ^ 42L;
            
            Assert.Equal((ulong)542251326, TransactionNumber.CalculateLockTime(obscured));
        }
        
        [Fact]
        public void CalculateNumberTest()
        {
            var localPaymentBasePoint =
                new ECKeyPair("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa", false);
            var remotePaymentBasePoint =
                new ECKeyPair("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991", false);

            ulong obscured = TransactionNumber.CalculateObscured(42, localPaymentBasePoint, remotePaymentBasePoint);
            ulong actual = TransactionNumber.CalculateNumber(obscured, localPaymentBasePoint, remotePaymentBasePoint);
            Assert.Equal(actual, (ulong)42);
        }
    }
}