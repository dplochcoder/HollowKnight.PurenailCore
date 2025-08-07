using PurenailCore.SystemUtil;
using System.Collections.Generic;
using UnityEngine;

namespace PurenailCore.CollectionUtil;

// O(log(N)) interval sum structure for efficiently getting sums from an ordered range.
// Supports append, remove from end, and arbitrary updates efficiently.
public class IntervalSums
{
    private readonly List<float> values = [];
    private readonly Dictionary<(int, int), float> rangeSums = [];

    public float Sum { get; private set; } = 0;

    public int Count => values.Count;

    public void Add(float value)
    {
        values.Add(value);
        UpdateSums(values.Count - 1);
        Sum += value;
    }

    public int SelectWeightedIndex(float weight)
    {
        int min = 0;
        int max = values.Count;

        int rangeSize = 2;
        while (rangeSize * 2 < max - min) rangeSize *= 2;

        while (max - min > 1)
        {
            var range = max - min;
            while (rangeSize >= range) rangeSize /= 2;
            var mid = min + rangeSize;

            var partial = (rangeSize == 1) ? values[min] : rangeSums[(min, mid)];
            if (weight < partial)
                max = mid;
            else
            {
                min = mid;
                weight -= partial;
            }
        }
        return min;
    }

    public int SelectWeightedIndex(System.Random? r = null) => SelectWeightedIndex(r?.NextFloat(Sum) ?? Random.Range(0, Sum));

    public float Last => values[values.Count - 1];

    public void RemoveLast()
    {
        foreach ((var a, var b) in RangesContaining(Count - 1)) rangeSums.Remove((a, b));

        Sum -= Last;
        values.RemoveAt(values.Count - 1);
    }

    public void Set(int index, float value)
    {
        var prev = values[index];
        Sum += value - prev;
        values[index] = value;

        UpdateSums(index);
    }

    public float this[int index]
    {
        get { return values[index]; }
        set { Set(index, value); }
    }

    public void Clear()
    {
        values.Clear();
        rangeSums.Clear();
        Sum = 0;
    }

    private static (int, int) RangeOfSize(int index, int size)
    {
        int floor = index / size * size;
        return (floor, floor + size);
    }

    private IEnumerable<(int, int)> RangesContaining(int index)
    {
        int size = 2;
        while (true)
        {
            (var a, var b) = RangeOfSize(index, size);
            if (b > Count) yield break;
            yield return (a, b);

            size *= 2;
        }
    }

    private void UpdateSums(int index)
    {
        foreach ((var a, var b) in RangesContaining(index))
        {
            var size = b - a;
            if (size == 2)
                rangeSums[(a, b)] = values[a] + values[a + 1];
            else
            {
                var mid = a + size / 2;
                rangeSums[(a, b)] = rangeSums[(a, mid)] + rangeSums[(mid, b)];
            }
        }
    }
}
