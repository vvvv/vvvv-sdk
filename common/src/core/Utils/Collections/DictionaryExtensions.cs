using System;
using System.Collections.Generic;

namespace VVVV.Utils.Collections
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// From http://www.haiders.net/post/Dictionary-Lookup-avoiding-KeyNotFound-Exception.aspx
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key">The key to look up.</param>
        /// <returns>The value associated with the key or default(TValue) of key not found.</returns>
        public static TValue ValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value = default(TValue);
            dictionary.TryGetValue(key, out value);
            return value;
        }
    }
}