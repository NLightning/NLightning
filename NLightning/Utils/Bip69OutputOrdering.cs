using System;
using System.Collections.Generic;
using NBitcoin;

namespace NLightning.Utils
{
    public class Bip69OutputOrdering : IComparer<TxOut>
    {
        public int Compare(TxOut x, TxOut y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentNullException();
            }

            if (x.Value == y.Value)
            {
                return String.Compare(x.ScriptPubKey.ToHex(), y.ScriptPubKey.ToHex(), StringComparison.Ordinal);
            }

            return x.Value.CompareTo(y.Value);
        }
    }
}