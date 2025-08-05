using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace PurenailCore.CollectionUtil;

public class IndexedSet<T> : IList<T>
{
    private readonly Dictionary<T, int> positions = [];
    private readonly SortedDictionary<int, T> elements = [];
    private int nextIndex = 0;

    private void Compact()
    {
        if (nextIndex == positions.Count) return;

        List<T> elems = [.. this];
        Clear();
        foreach (var e in elems) Add(e);
    }

    public T Get(int index)
    {
        if (index < 0 || index >= positions.Count) throw new System.IndexOutOfRangeException($"{index}: [0, {positions.Count})");

        Compact();
        return elements[index];
    }

    public bool Contains(T item) => positions.ContainsKey(item);

    public bool Add(T item)
    {
        if (positions.ContainsKey(item)) return false;

        positions.Add(item, nextIndex);
        elements.Add(nextIndex, item);
        nextIndex++;
        return true;
    }

    public bool Remove(T item)
    {
        if (!positions.TryGetValue(item, out var idx)) return false;

        positions.Remove(item);
        elements.Remove(idx);
        return true;
    }

    public void Remove(int idx) => Remove(Get(idx));

    public void Clear()
    {
        positions.Clear();
        elements.Clear();
        nextIndex = 0;
    }

    [JsonIgnore]
    public int Count => positions.Count;

    [JsonIgnore]
    public bool IsReadOnly => false;

    public T this[int index]
    {
        get
        {
            Compact();
            return elements[index];
        }
        set
        {
            Compact();

            var prev = elements[index];
            if (positions.TryGetValue(value, out var prevIdx))
            {
                if (prevIdx != index)
                {
                    (elements[index], elements[prevIdx]) = (value, prev);
                    positions[prev] = prevIdx;
                    positions[value] = index;
                }
                return;
            }

            positions.Remove(prev);
            positions[value] = index;
            elements[index] = value;
        }
    }

    public IEnumerator<T> GetEnumerator() => elements.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => elements.Values.GetEnumerator();

    public int IndexOf(T item)
    {
        Compact();
        return positions.TryGetValue(item, out var idx) ? idx : -1;
    }

    private void ModifyList(System.Action<List<T>> action)
    {
        List<T> list = [.. this];
        action(list);
        Clear();
        list.ForEach(i => Add(i));
    }

    public void Insert(int index, T item)
    {
        if (Contains(item)) return;
        ModifyList(l => l.Insert(index, item));
    }

    public void RemoveAt(int index)
    {
        Compact();
        positions.Remove(elements[index]);
        elements.Remove(index);
    }

    void ICollection<T>.Add(T item) => Add(item);

    public void CopyTo(T[] array, int arrayIndex)
    {
        List<T> list = [.. elements.Values];
        list.CopyTo(array, arrayIndex);
    }
}

public class IndexedWeightedSet<T>
{
    private readonly IndexedSet<T> set = [];
    private readonly IntervalSums sums = new();

    public int Count => set.Count;

    public float TotalWeight => sums.Sum;

    public bool Contains(T item) => set.Contains(item);

    public void Add(T item, float weight = 1)
    {
        if (set.Add(item)) sums.Add(weight);
        else
        {
            int idx = set.IndexOf(item);
            sums[idx] = sums[idx] + weight;
        }
    }

    private void RemoveIndex(int idx)
    {
        set.Remove(idx);
        sums[idx] = sums.Last;
        sums.RemoveLast();
    }

    public float Remove(T elem)
    {
        int idx = set.IndexOf(elem);
        if (idx == -1) return 0;

        float weight = sums[idx];
        RemoveIndex(idx);
        return weight;
    }

    public T Take(System.Random r)
    {
        var idx = sums.SelectWeightedIndex(r);

        T elem = set[idx];
        RemoveIndex(idx);
        return elem;
    }

    public T Sample(System.Random r)
    {
        var idx = sums.SelectWeightedIndex(r);
        return set[idx];
    }

    public void Clear()
    {
        set.Clear();
        sums.Clear();
    }
}
