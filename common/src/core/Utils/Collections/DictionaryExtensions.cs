using System;
using System.Collections.Generic;

namespace System.Collections.Generic
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
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
                value = default(TValue);
            return value;
        }

        /// <summary>
        /// create the value if not already stored for that key
        /// </summary>
        public static TValue EnsureValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> creator)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = creator(key);
                dictionary[key] = value;
            }
            return value;
        }

        public static IEnumerable<TValue> FirstValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
                yield return value;
        }
    }
}