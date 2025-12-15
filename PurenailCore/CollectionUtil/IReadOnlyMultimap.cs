using System.Collections.Generic;

namespace PurenailCore.CollectionUtil;

public interface IReadOnlyMultimap<K, V> : IEnumerable<(K, IReadOnlyCollection<V>)>
{
    int KeyCount { get; }

    bool Contains(K key, V value);

    IEnumerable<K> Keys { get; }

    bool TryGet(K key, out IReadOnlyCollection<V> values);

    IReadOnlyCollection<V> Get(K key);
}
