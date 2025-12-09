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

    public void Add(T item) => Add(item, 1);

    public void Add(T item, int count)
    {
        if (count <= 0) return;
        if (!counts.TryGetValue(item, out int current)) current = 0;
        counts[item] = current + count;

        total += count;
    }

    public void Add(IEnumerable<T> items) => items.ForEach(Add);

    public bool Remove(T item) => Remove(item, 1);
    
    public bool Remove(T item, int count)
    {
        if (count <= 0) return false;
        if (!counts.TryGetValue(item, out int current)) return false;

        if (count >= current)
        {
            counts.Remove(item);
            total -= current;
        }
        else
        {
            total -= count;
            counts[item] = current - count;
        }

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
