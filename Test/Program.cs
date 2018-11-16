using System;
using System.Linq;
using NBitcoin;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            PubKey fundingPubKey1 = new PubKey("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb");
            PubKey fundingPubKey2 = new PubKey("030e9f7b623d2ccc7c9bd44d66d5ce21ce504c0acf6385a132cec6d3c39fa711c1");
            var secret = Key.Parse("cRCH7YNcarfvaiY1GWUKQrRGmoezvfAiqHtdRvxe16shzbd7LDMz", Network.TestNet);
            var input = Transaction.Parse("01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff03510101ffffffff0100f2052a010000001976a9143ca33c2e4446f4a305f23c80df8ad1afdcf652f988ac00000000", NBitcoin.Network.TestNet);
            var multiSigPubKey = PayToMultiSigTemplate.Instance.GenerateScriptPubKey(2, fundingPubKey1, fundingPubKey2);
            var coins = input.Outputs.AsCoins().First();
            
            TransactionBuilder builder = new TransactionBuilder();
            Transaction fundingTransaction = 
                builder
                    .AddCoins(coins)
                    .Send(multiSigPubKey.WitHash.ScriptPubKey, Money.Satoshis(10000000))
                    .AddKeys(secret)
                    .SendFees(Money.Satoshis(13920))
                    .SetChange(secret.PubKey.WitHash)
                    .SetConsensusFactory(Network.TestNet)
                    .BuildTransaction(sign: true);
            
            //fundingTransaction.Version = 2;

            Console.WriteLine($"Result: {fundingTransaction}");
            Console.WriteLine($"Expected: {Transaction.Parse("0200000001adbb20ea41a8423ea937e76e8151636bf6093b70eaff942930d20576600521fd000000006b48304502210090587b6201e166ad6af0227d3036a9454223d49a1f11839c1a362184340ef0240220577f7cd5cca78719405cbf1de7414ac027f0239ef6e214c90fcaab0454d84b3b012103535b32d5eb0a6ed0982a0479bbadc9868d9836f6ba94dd5a63be16d875069184ffffffff028096980000000000220020c015c4a6be010e21657068fc2e6a9d02b27ebe4d490a25846f7237f104d1a3cd20256d29010000001600143ca33c2e4446f4a305f23c80df8ad1afdcf652f900000000", Network.TestNet)}");

            Console.WriteLine("Is valid: " + builder.Verify(fundingTransaction));
        }
    }
}