using System.Collections.Generic;

namespace PurenailCore.CollectionUtil;

public interface IMultimap<K, V> : IEnumerable<(K, IReadOnlyCollection<V>)>
{
    bool Contains(K key, V value);

    IEnumerable<K> Keys { get; }

    bool TryGet(K key, out IEnumerable<V> values);

    IEnumerable<V> Get(K key);

    bool Add(K key, V value);

    bool Remove(K key, V value);
}
