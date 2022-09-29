using System;
using System.Collections.Generic;

namespace PurenailCore.SystemUtil
{
    public static class Extensions
    {
        public delegate V Supplier<V>();

        public static void AddIfEmpty<K, V>(this IDictionary<K, V> dict, K key, Supplier<V> creator)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = creator.Invoke();
            }
        }

        public static V GetOrAddNew<K,V>(this IDictionary<K, V> self, K key) where V : new()
        {
            if (self.TryGetValue(key, out V value)) return value;

            value = new();
            self[key] = value;
            return value;
        }

        public static V GetOrDefault<K, V>(this IDictionary<K, V> dict, K key, Supplier<V> creator)
        {
            if (dict.TryGetValue(key, out V value))
            {
                return value;
            }

            return creator.Invoke();
        }

        public static void Shuffle<T>(this List<T> list, Random r)
        {
            for (int i = 0; i < list.Count - 1; ++i)
            {
                int j = i + r.Next(0, list.Count - i);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
