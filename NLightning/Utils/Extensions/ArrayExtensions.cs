using System;
using System.Linq;

namespace NLightning.Utils.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] ConcatToNewArray<T>(this T[] x, T[] y)
        {
            T[] newArray = new T[x.Length + y.Length];
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");
            Array.Copy(x, 0, newArray, 0, x.Length);
            Array.Copy(y, 0, newArray, x.Length, y.Length);
            
            return newArray;
        }
       
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}