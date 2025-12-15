using System.Collections.Generic;

namespace PurenailCore.CollectionUtil;

public class IndexedSortedMultimap<K, V>
{
    private readonly IndexedSortedDictionary<K, HashSet<V>> dict = [];

    private HashSet<V> GetOrAddNew(K key)
    {
        if (dict.TryGetValue(key, out var set)) return set;
        else if (dict.TryGetLowerBound(key, out K _, out set))
        {
            HashSet<V> newSet = [.. set];
            dict.Add(key, newSet);
            return newSet;
        }
        else
        {
            set = [];
            dict.Add(key, set);
            return set;
        }
    }

    private bool KeysEqual(K key1, K key2) => EqualityComparer<K>.Default.Equals(key1, key2);

    private bool KeyLessThan(K key1, K key2) => Comparer<K>.Default.Compare(key1, key2) < 0;

    public void Add(K beginInclusive, K endExclusive, V value)
    {
        if (!dict.TryGetLowerBound(beginInclusive, out K boundKey, out var set) || (KeyLessThan(boundKey, beginInclusive) && !set.Contains(value))) GetOrAddNew(beginInclusive);
        if (!dict.TryGetLowerBound(endExclusive, out boundKey, out set) || (KeyLessThan(boundKey, endExclusive) && !set.Contains(value))) GetOrAddNew(endExclusive);

        foreach (var e in dict.GetViewBetween(beginInclusive, endExclusive))
        {
            if (KeysEqual(e.Key, endExclusive)) continue;
            e.Value.Add(value);
        }
    }

    public void Remove(K beginInclusive, K endExclusive, V value)
    {
        if (!dict.TryGetLowerBound(beginInclusive, out K boundKey, out var set) || (KeyLessThan(boundKey, beginInclusive) && set.Contains(value))) GetOrAddNew(beginInclusive);
        if (!dict.TryGetLowerBound(endExclusive, out boundKey, out set) || (KeyLessThan(boundKey, endExclusive) && set.Contains(value))) GetOrAddNew(endExclusive);

        foreach (var e in dict.GetViewBetween(beginInclusive, endExclusive))
        {
            if (KeysEqual(e.Key, endExclusive)) continue;
            e.Value.Remove(value);
        }
    }

    private static readonly List<V> EmptyList = [];

    public IEnumerable<V> Get(K key)
    {
        if (dict.TryGetLowerBound(key, out K _, out var set)) return set;
        else return EmptyList;
    }

    public void Coalesce()
    {
        List<(K, HashSet<V>)> replace = [];
        HashSet<V> previous = [];

        foreach (var (k, v) in dict)
        {
            if (v.SetEquals(previous)) continue;

            replace.Add((k, v));
            previous = v;
        }

        dict.Clear();
        foreach (var (k, v) in replace) dict[k] = v;
    }

    public void Clear() => dict.Clear();
}
