using System;
using System.Collections.Generic;

namespace PurenailCore.CollectionUtil;

public class SortedMultimap<K, V> where K : IComparable<K>
{
    private readonly SortedDictionary<K, HashSet<V>> dict = [];

    public SortedDictionary<K, HashSet<V>> AsDict
    {
        get
        {
            var ret = new SortedDictionary<K, HashSet<V>>();
            foreach (var e in dict) ret.Add(e.Key, [.. e.Value]);
            return ret;
        }
        set
        {
            dict.Clear();
            foreach (var e in value) dict.Add(e.Key, [.. e.Value]);
        }
    }

    public bool Contains(K key, V value) => dict.TryGetValue(key, out var values) && values.Contains(value);

    private static readonly List<V> EmptyList = [];

    public bool TryGet(K key, out IEnumerable<V> values)
    {
        if (dict.TryGetValue(key, out var set))
        {
            values = set;
            return true;
        }
        else
        {
            values = EmptyList;
            return false;
        }
    }

    public IEnumerable<V> Get(K key)
    {
        if (dict.TryGetValue(key, out var values)) return values;
        else return EmptyList;
    }

    public bool Add(K key, V value)
    {
        if (!dict.TryGetValue(key, out var values))
        {
            dict.Add(key, [value]);
            return true;
        }

        return values.Add(value);
    }

    public bool Remove(K key, V value)
    {
        if (!dict.TryGetValue(key, out var values)) return false;
        if (values.Remove(value))
        {
            if (values.Count == 0) dict.Remove(key);
            return true;
        }

        return false;
    }
}
