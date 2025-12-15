using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PurenailCore.CollectionUtil;

public interface IIndexedSortedMultimap<K, V> : IReadOnlyMultimap<K, V>
{
    bool Empty { get; }

    (K, IReadOnlyCollection<V>) Min { get; }
    (K, IReadOnlyCollection<V>) Max { get; }

    bool TryGetLowerBound(K key, out K boundKey, out IReadOnlyCollection<V> value);
    bool TryGetUpperBound(K key, out K boundKey, out IReadOnlyCollection<V> value);
}

public class IndexedSortedMultimap<K, V> : IIndexedSortedMultimap<K, V>, IMultimap<K, V>
{
    private class View(IIndexedSortedDictionary<K, HashSet<V>> view) : IIndexedSortedMultimap<K, V>
    {
        public int KeyCount => view.Count;

        public bool Empty => view.Empty;

        public (K, IReadOnlyCollection<V>) Min => view.Min;

        public (K, IReadOnlyCollection<V>) Max => view.Max;

        public IEnumerable<K> Keys => view.Keys;

        public bool Contains(K key, V value) => view.TryGetValue(key, out var set) && set.Contains(value);

        public IReadOnlyCollection<V> Get(K key) => view.TryGetValue(key, out var set) ? set : EmptyCollection<V>.Instance;

        public bool TryGet(K key, out IReadOnlyCollection<V> values)
        {
            if (view.TryGetValue(key, out var set))
            {
                values = set;
                return true;
            }

            values = EmptyCollection<V>.Instance;
            return false;
        }

        public bool TryGetLowerBound(K key, out K boundKey, out IReadOnlyCollection<V> value)
        {
            if (view.TryGetLowerBound(key, out boundKey, out var set))
            {
                value = set;
                return true;
            }

            value = EmptyCollection<V>.Instance;
            return false;
        }

        public bool TryGetUpperBound(K key, out K boundKey, out IReadOnlyCollection<V> value)
        {
            if (view.TryGetUpperBound(key, out boundKey, out var set))
            {
                value = set;
                return true;
            }

            value = EmptyCollection<V>.Instance;
            return false;
        }

        private IEnumerator<(K, IReadOnlyCollection<V>)> GetEnumeratorInternal()
        {
            foreach (var e in view)
                yield return (e.Key, e.Value);
        }

        public IEnumerator<(K, IReadOnlyCollection<V>)> GetEnumerator() => GetEnumeratorInternal();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();
    }

    private readonly IndexedSortedDictionary<K, HashSet<V>> dict = [];

    public int KeyCount => dict.Count;

    public bool Empty => dict.Empty;

    public void Clear() => dict.Clear();

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

    public bool Contains(K key, V value) => dict.TryGetValue(key, out var set) && set.Contains(value);

    public (K, IReadOnlyCollection<V>) Min => dict.Min;

    public (K, IReadOnlyCollection<V>) Max => dict.Max;

    public bool TryGet(K key, out IReadOnlyCollection<V> values)
    {
        if (dict.TryGetValue(key, out var set))
        {
            values = set;
            return true;
        }

        values = EmptyCollection<V>.Instance;
        return false;
    }

    public IReadOnlyCollection<V> Get(K key)
    {
        if (dict.TryGetLowerBound(key, out K _, out var set)) return set;
        else return EmptyCollection<V>.Instance;
    }

    public bool Add(K key, V value) => GetOrAddNew(key).Add(value);

    public bool Remove(K key, V value)
    {
        if (!dict.TryGetValue(key, out var set) || !set.Remove(value))
            return false;

        if (set.Count == 0) dict.Remove(key);
        return true;
    }

    public IEnumerable<K> Keys => dict.Keys;

    public bool TryGetLowerBound(K key, out K boundKey, out IReadOnlyCollection<V> value)
    {
        if (dict.TryGetLowerBound(key, out boundKey, out var set))
        {
            value = set;
            return true;
        }

        value = EmptyCollection<V>.Instance;
        return false;
    }

    public bool TryGetUpperBound(K key, out K boundKey, out IReadOnlyCollection<V> value)
    {
        if (dict.TryGetUpperBound(key, out boundKey, out var set))
        {
            value = set;
            return true;
        }

        value = EmptyCollection<V>.Instance;
        return false;
    }

    public IIndexedSortedMultimap<K, V> GetViewBetween(K left, K right) => new View(dict.GetViewBetween(left, right));

    private IEnumerator<(K, IReadOnlyCollection<V>)> GetEnumeratorInternal() => dict.Select(p => (p.Key, (IReadOnlyCollection<V>)p.Value)).GetEnumerator();

    public IEnumerator<(K, IReadOnlyCollection<V>)> GetEnumerator() => GetEnumeratorInternal();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();
}
