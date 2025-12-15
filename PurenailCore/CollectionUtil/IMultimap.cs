namespace PurenailCore.CollectionUtil;

public interface IMultimap<K, V> : IReadOnlyMultimap<K, V>
{
    bool Add(K key, V value);

    bool Remove(K key, V value);

    void Clear();
}
