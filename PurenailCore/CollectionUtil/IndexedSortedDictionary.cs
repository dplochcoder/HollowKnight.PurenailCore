using System.Collections;
using System.Collections.Generic;

namespace PurenailCore.CollectionUtil;

public interface IIndexedSortedDictionary<K, V> : IEnumerable<(K, V)>
{
    bool Empty { get; }
    int Count { get; }

    (K, V) Min { get; }
    (K, V) Max { get; }

    public IEnumerable<K> Keys { get; }
    public IEnumerable<V> Values { get; }

    bool TryGetLowerBound(K key, out K boundKey, out V value);
    bool TryGetUpperBound(K key, out K boundKey, out V value);
}

public class IndexedSortedDictionary<K, V> : IIndexedSortedDictionary<K, V>
{
    private class View : IIndexedSortedDictionary<K, V>
    {
        private readonly IndexedSortedDictionary<K, V> dict;
        private readonly SortedSet<K> keyView;

        internal View(IndexedSortedDictionary<K, V> dict, SortedSet<K> keyView)
        {
            this.dict = dict;
            this.keyView = keyView;
        }

        public bool Empty => !keyView.GetEnumerator().MoveNext();

        public int Count => keyView.Count;

        public (K, V) Min => (keyView.Min, dict[keyView.Min]);

        public (K, V) Max => (keyView.Max, dict[keyView.Max]);

        public bool TryGetLowerBound(K key, out K boundKey, out V value)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            boundKey = default;
            value = default;
#pragma warning restore CS8601 // Possible null reference assignment.

            if (Empty) return false;
            if (Comparer<K>.Default.Compare(key, keyView.Min) < 0) return false;

            var view = keyView.GetViewBetween(keyView.Min, key);
            if (!view.GetEnumerator().MoveNext()) return false;

            boundKey = view.Max;
            value = dict[view.Max];
            return true;
        }

        public bool TryGetUpperBound(K key, out K boundKey, out V value)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            boundKey = default;
            value = default;
#pragma warning restore CS8601 // Possible null reference assignment.

            if (Empty) return false;
            if (Comparer<K>.Default.Compare(key, keyView.Max) > 0) return false;

            var view = keyView.GetViewBetween(key, keyView.Max);
            if (!view.GetEnumerator().MoveNext()) return false;

            boundKey = view.Max;
            value = dict[view.Max];
            return true;
        }
        public IEnumerable<K> Keys => keyView;

        public IEnumerable<V> Values
        {
            get
            {
                foreach (var key in keyView) yield return dict[key];
            }
        }

        private IEnumerator<(K, V)> GetEnumeratorImpl()
        {
            foreach (var key in keyView) yield return (key, dict[key]);
        }

        public IEnumerator<(K, V)> GetEnumerator() => GetEnumeratorImpl();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorImpl();
    }

    private readonly SortedDictionary<K, V> dict = [];
    private readonly SortedSet<K> keys = [];

    public IndexedSortedDictionary() { }

    public IndexedSortedDictionary(IEnumerable<(K, V)> items)
    {
        foreach (var (k, v) in items) Add(k, v);
    }

    public IndexedSortedDictionary(IReadOnlyDictionary<K, V> dict)
    {
        foreach (var e in dict) Add(e.Key, e.Value);
    }

    public void Add(K key, V value)
    {
        keys.Add(key);
        dict[key] = value;
    }

    public bool Remove(K key)
    {
        if (!keys.Remove(key)) return false;

        dict.Remove(key);
        return true;
    }

    public bool Empty => !keys.GetEnumerator().MoveNext();

    public int Count => keys.Count;

    public (K, V) Min => (keys.Min, dict[keys.Min]);

    public (K, V) Max => (keys.Max, dict[keys.Max]);

    public bool TryGetValue(K key, out V value) => dict.TryGetValue(key, out value);

    public V this[K key]
    {
        get { return dict[key]; }
        set { Add(key, value); }
    }

    public void Clear()
    {
        keys.Clear();
        dict.Clear();
    }

    public bool TryGetLowerBound(K key, out K boundKey, out V value)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        boundKey = default;
        value = default;
#pragma warning restore CS8601 // Possible null reference assignment.

        if (Empty) return false;
        if (Comparer<K>.Default.Compare(key, keys.Min) < 0) return false;

        var view = keys.GetViewBetween(keys.Min, key);
        if (!view.GetEnumerator().MoveNext()) return false;

        boundKey = view.Max;
        value = dict[view.Max];
        return true;
    }

    public bool TryGetUpperBound(K key, out K boundKey, out V value)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        boundKey = default;
        value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
        if (Empty) return false;
        if (Comparer<K>.Default.Compare(key, keys.Max) > 0) return false;

        var view = keys.GetViewBetween(key, keys.Max);
        if (!view.GetEnumerator().MoveNext()) return false;

        boundKey = view.Min;
        value = dict[view.Min];
        return true;
    }

    public IIndexedSortedDictionary<K, V> GetViewBetween(K left, K right) => new View(this, keys.GetViewBetween(left, right));

    public IEnumerable<K> Keys => dict.Keys;

    public IEnumerable<V> Values => dict.Values;

    private IEnumerator<(K, V)> GetEnumeratorImpl()
    {
        foreach (var e in dict) yield return (e.Key, e.Value);
    }

    public IEnumerator<(K, V)> GetEnumerator() => GetEnumeratorImpl();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorImpl();
}

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

        foreach (var (k, v) in dict.GetViewBetween(beginInclusive, endExclusive))
        {
            if (KeysEqual(k, endExclusive)) continue;
            v.Add(value);
        }
    }

    public void Remove(K beginInclusive, K endExclusive, V value)
    {
        if (!dict.TryGetLowerBound(beginInclusive, out K boundKey, out var set) || (KeyLessThan(boundKey, beginInclusive) && set.Contains(value))) GetOrAddNew(beginInclusive);
        if (!dict.TryGetLowerBound(endExclusive, out boundKey, out set) || (KeyLessThan(boundKey, endExclusive) && set.Contains(value))) GetOrAddNew(endExclusive);

        foreach (var (k, v) in dict.GetViewBetween(beginInclusive, endExclusive))
        {
            if (KeysEqual(k, endExclusive)) continue;
            v.Remove(value);
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
