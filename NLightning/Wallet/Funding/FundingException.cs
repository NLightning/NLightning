using System;

namespace NLightning.Wallet.Funding
{
    public class FundingException : Exception
    {
        public FundingException(string message) : base(message)
        {
        }
    }
}