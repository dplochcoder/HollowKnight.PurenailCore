using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PurenailCore.CollectionUtil;

public interface IIndexedSortedDictionary<K, V> : IReadOnlyDictionary<K, V>
{
    bool Empty { get; }

    (K, V) Min { get; }
    (K, V) Max { get; }

    bool TryGetLowerBound(K key, out K boundKey, out V value);
    bool TryGetUpperBound(K key, out K boundKey, out V value);
}

public class IndexedSortedDictionary<K, V> : IIndexedSortedDictionary<K, V>, IDictionary<K, V>
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

        public V this[K key] => keyView.Contains(key) ? dict[key] : throw new KeyNotFoundException($"{key}");

        public bool ContainsKey(K key) => keyView.Contains(key);

        public bool TryGetValue(K key, out V value)
        {
            if (keyView.Contains(key))
            {
                value = dict[key];
                return true;
            }

#pragma warning disable CS8601 // Possible null reference assignment.
            value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
            return false;
        }

        private IEnumerator<(K, V)> GetEnumeratorInternal()
        {
            foreach (var key in keyView) yield return (key, dict[key]);
        }

        public IEnumerator<(K, V)> GetEnumerator() => GetEnumeratorInternal();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();

        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() => keyView.Select(k => new KeyValuePair<K, V>(k, dict[k])).GetEnumerator();
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

    public void Add(KeyValuePair<K, V> item) => Add(item.Key, item.Value);

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

    public bool ContainsKey(K key) => dict.ContainsKey(key);

    public bool Contains(KeyValuePair<K, V> item) => dict.Contains(item);

    public bool Remove(KeyValuePair<K, V> item) => dict.Contains(item) && dict.Remove(item.Key);

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

    public ICollection<K> Keys => dict.Keys;

    public ICollection<V> Values => dict.Values;

    public bool IsReadOnly => false;

    IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => Keys;

    IEnumerable<V> IReadOnlyDictionary<K, V>.Values => Values;

    private IEnumerator<(K, V)> GetEnumeratorInternal()
    {
        foreach (var e in dict) yield return (e.Key, e.Value);
    }

    public IEnumerator<(K, V)> GetEnumerator() => GetEnumeratorInternal();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();

    IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() => dict.GetEnumerator();

    public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) => dict.CopyTo(array, arrayIndex);
}
