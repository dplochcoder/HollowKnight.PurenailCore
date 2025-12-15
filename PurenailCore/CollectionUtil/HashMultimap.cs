using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PurenailCore.CollectionUtil;

// Dictionary that can map multiple values to a single key.
public class HashMultimap<K, V> : IMultimap<K, V>
{
    private readonly Dictionary<K, HashSet<V>> dict = [];

    public int KeyCount => dict.Count;

    public IEnumerable<K> Keys => dict.Keys;

    public bool Contains(K key, V value) => dict.TryGetValue(key, out var values) && values.Contains(value);

    public bool TryGet(K key, out IEnumerable<V> values)
    {
        if (dict.TryGetValue(key, out var set))
        {
            values = set;
            return true;
        }
        else
        {
            values = EmptyCollection<V>.Instance;
            return false;
        }
    }

    public IEnumerable<V> Get(K key)
    {
        if (dict.TryGetValue(key, out var values)) return values;
        else return EmptyCollection<V>.Instance;
    }

    public void Clear() => dict.Clear();

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

    private IEnumerator<(K, IReadOnlyCollection<V>)> GetEnumeratorInternal() => dict.Select(e => (e.Key, (IReadOnlyCollection<V>)e.Value)).GetEnumerator();

    public IEnumerator<(K, IReadOnlyCollection<V>)> GetEnumerator() => GetEnumeratorInternal();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();

    public bool TryGet(K key, out IReadOnlyCollection<V> values)
    {
        throw new System.NotImplementedException();
    }

    IReadOnlyCollection<V> IReadOnlyMultimap<K, V>.Get(K key)
    {
        throw new System.NotImplementedException();
    }
}
