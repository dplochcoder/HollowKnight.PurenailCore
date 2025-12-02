using System;
using System.Collections.Generic;
using System.Linq;

namespace PurenailCore.SystemUtil;

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

    public static bool EnumeratorEqual<T>(this IEnumerator<T> self, IEnumerator<T> other, Func<T, T, bool>? equalsFn = null)
    {
        equalsFn ??= EqualityComparer<T>.Default.Equals;
        while (true)
        {
            bool more = self.MoveNext();
            if (other.MoveNext() != more) return false;
            if (!more) return true;
            if (!equalsFn(self.Current, other.Current)) return false;
        }
    }

    public static void Shuffle<T>(this List<T> self, Random r)
    {
        for (int i = 0; i < self.Count - 1; ++i)
        {
            int j = i + r.Next(0, self.Count - i);
            (self[i], self[j]) = (self[j], self[i]);
        }
    }

    public static void SortBy<T, C>(this List<T> self, Func<T, C> extractor) where C : IComparable<C>
    {
        List<(T, C)> pairs = [.. self.Select(t => (t, extractor(t)))];
        pairs.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        self.Clear();
        self.AddRange([.. pairs.Select(p => p.Item1)]);
    }

    public static float NextFloat(this Random self, float min, float max) => min + (max - min) * (float) self.NextDouble();

    public static float NextFloat(this Random self, float max) => self.NextFloat(0, max);

    public static float NextFloat(this Random self) => self.NextFloat(0f, 1f);

    public static void ForEach<T>(this IEnumerable<T> iter, Action<T> action)
    {
        foreach (var t in iter)
        {
            action(t);
        }
    }
}
