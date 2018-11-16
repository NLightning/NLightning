using System.Collections.Generic;
using NBitcoin;

namespace NLightning.Wallet.Funding
{
    public class FundingTransaction
    {
        public FundingTransaction(Transaction transaction, ushort fundingOutputIndex, List<Coin> inputCoins, byte[] finalPubKeyScript)
        {
            Transaction = transaction;
            FundingOutputIndex = fundingOutputIndex;
            FinalPubKeyScript = finalPubKeyScript;
            InputCoins = inputCoins.AsReadOnly();
        }
        
        public Transaction Transaction { get; }
        public ushort FundingOutputIndex { get; }
        public IReadOnlyCollection<Coin> InputCoins { get; }
        public byte[] FinalPubKeyScript { get; }
     }
}