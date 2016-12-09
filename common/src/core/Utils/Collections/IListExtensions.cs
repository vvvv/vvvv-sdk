using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Utils.Collections
{
    public static class IListExtensions
    {
        /// <summary>
        /// throws if input is empty. so please check that first
        /// </summary>
        public static T ClampedElementAtIfNotEmpty<T>(this IList<T> input, ref int index)
        {
            index = index < 0 ? 0 : index > input.Count - 1 ? input.Count - 1 : index;
            return input[index];
        }

        /// <summary>
        /// if count = 0 returns a default(T)
        /// </summary>
        public static T ClampedElementAtOrDefault<T>(this IList<T> input, ref int index)
        {
            if (input.Count == 0)
                return default(T);
            index = index < 0 ? 0 : index > input.Count - 1 ? input.Count - 1 : index;
            return input[index];
        }
    }
}
