using NLightning.Cryptography;

namespace NLightning.Wallet.Commitment.Models
{
    public class Htlc
    {
        private byte[] _paymentPreImage;

        public Htlc()
        {
        }
        
        public Htlc(Direction direction, ulong amountMsat, uint expiry, byte[] paymentPreImage)
        {
            Direction = direction;
            AmountMsat = amountMsat;
            Expiry = expiry;
            PaymentPreImage = paymentPreImage;
        }

        public int Id { get; set; }
        public Direction Direction { get; set; }
        public byte[] PaymentHash { get; set; }
        public ulong AmountMsat { get; set; }
        public uint Expiry { get; set; }
        public byte[] PaymentPreImage { 
        
            get { return _paymentPreImage; }
            
            set
            {
                _paymentPreImage = value;
                PaymentHash = SHA256.ComputeHash(_paymentPreImage);
            }
        }
    }
}