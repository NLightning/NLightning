using NBitcoin;

namespace NLightning.Wallet.KeyDerivation
{
    /*
     * Our key derivation hierarchy is
     * m/1017'/coinType'/keyFamily/0/index
     */
    public class KeyPathHelper
    {
        public const int KeyDerivationVersion = 0;
        public const int Bip043Purpose = 1017;

        public static KeyPath Create(int coinType, KeyFamily keyFamily, int index)
        {
            return KeyPath.Parse($"m/{Bip043Purpose}'/{coinType}/{(int)keyFamily}/{KeyDerivationVersion}/{index}");
        }
    }
}