using System;
using System.Collections.Generic;

namespace Klavogonki
{
    public static class DictionaryHelpers
    {
        public static TValue GetValueOrDefault<TKey, TValue>
            (this IDictionary<TKey, TValue> dictionary,
             TKey key)
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : default(TValue);
        }

        public static TValue GetValueOrDefault<TKey, TValue>
            (this IDictionary<TKey, TValue> dictionary,
             TKey key,
             TValue defaultValue)
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>
            (this IDictionary<TKey, TValue> dictionary,
             TKey key,
             Func<TValue> defaultValueProvider)
        {
            return dictionary.TryGetValue(key, out TValue value) ? value
                 : defaultValueProvider();
        }
    }
}
