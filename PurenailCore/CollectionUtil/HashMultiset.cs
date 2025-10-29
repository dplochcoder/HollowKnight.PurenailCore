using PurenailCore.SystemUtil;
using System.Collections.Generic;

namespace PurenailCore.CollectionUtil;

// HashSet that can store duplicate values.
public class HashMultiset<T>
{
    private readonly Dictionary<T, int> counts = [];
    private int total;

    public int Count => total;

    public bool Contains(T item) => counts.ContainsKey(item);

    public int CountOf(T item) => counts.TryGetValue(item, out var count) ? count : 0;

    public IEnumerable<T> Distinct() => counts.Keys;

    public int NumDistinct() => counts.Count;

    public void Clear() => counts.Clear();

    public void Add(T item)
    {
        if (!counts.TryGetValue(item, out int count)) count = 0;
        counts[item] = count + 1;

        ++total;
    }

    public void Add(IEnumerable<T> items) => items.ForEach(Add);

    public bool Remove(T item)
    {
        if (!counts.TryGetValue(item, out int count)) return false;

        if (--count == 0) counts.Remove(item);
        else counts[item] = count;

        --total;
        return true;
    }

    public bool RemoveAll(T item)
    {
        if (!counts.TryGetValue(item, out int count)) return false;

        counts.Remove(item);
        total -= count;
        return true;
    }

    public bool Remove(IEnumerable<T> items)
    {
        bool changed = false;
        foreach (var item in items) changed |= Remove(item);
        return changed;
    }
}
