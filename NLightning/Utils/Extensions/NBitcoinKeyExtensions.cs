using System.Reflection;
using NBitcoin;

namespace NLightning.Utils.Extensions
{
    public static class NBitcoinKeyExtensions
    {
        public static byte[] GetBytes(this Key key)
        {
            // TODO: ask NBitcoin to expose raw key data
            return (byte[])key.GetType().GetField("vch", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(key);
        }
    }
}