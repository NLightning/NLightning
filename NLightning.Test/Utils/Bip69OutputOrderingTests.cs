using System.Collections.Generic;
using NBitcoin;
using NLightning.Utils;
using Xunit;

namespace NLightning.Test.Utils
{
    public class Bip69OutputOrderingTests
    {
        [Fact]
        public void OrderingTest1()
        {
            TxOut txOut1 = new TxOut(Money.Satoshis(1000), new Script("0003"));
            TxOut txOut2 = new TxOut(Money.Satoshis(1000), new Script("0002"));
            TxOut txOut3 = new TxOut(Money.Satoshis(999), new Script("0001"));
            TxOut txOut4 = new TxOut(Money.Satoshis(999), new Script("1000"));
            TxOut txOut5 = new TxOut(Money.Satoshis(50), new Script("200000"));
            TxOut txOut6 = new TxOut(Money.Satoshis(51), new Script("F000"));
            TxOut txOut7 = new TxOut(Money.Satoshis(150), new Script("F00100000000"));
            TxOut txOut8 = new TxOut(Money.Satoshis(150), new Script("0006"));
            TxOut txOut9 = new TxOut(Money.Satoshis(1000), new Script("FF03"));
            TxOut txOut10 = new TxOut(Money.Satoshis(1000), new Script("FF02"));
            
            List<TxOut> list = new List<TxOut>();

            list.Add(txOut1);
            list.Add(txOut2);
            list.Add(txOut3);
            list.Add(txOut4);
            list.Add(txOut5);
            list.Add(txOut6);
            list.Add(txOut7);
            list.Add(txOut8);
            list.Add(txOut9);
            list.Add(txOut10);
            
            list.Sort(new Bip69OutputOrdering());
            
            Assert.Equal(txOut5, list[0]);
            Assert.Equal(txOut6, list[1]);
            Assert.Equal(txOut8, list[2]);
            Assert.Equal(txOut7, list[3]);
            Assert.Equal(txOut3, list[4]);
            Assert.Equal(txOut4, list[5]);
            Assert.Equal(txOut2, list[6]);
            Assert.Equal(txOut1, list[7]);
            Assert.Equal(txOut10, list[8]);
            Assert.Equal(txOut9, list[9]);
        }
        
    }
}