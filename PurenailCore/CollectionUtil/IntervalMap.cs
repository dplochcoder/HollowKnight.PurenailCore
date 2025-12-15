using PurenailCore.SystemUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PurenailCore.CollectionUtil;

public class IntervalMap<T> : IEnumerable<(Interval, T)>, IEquatable<IntervalMap<T>>
{
    private record Entry(Interval Range, T Value)
    {
        public readonly Interval Range = Range;
        public readonly T Value = Value;

        public float Key => Range.Min;
    }

    private readonly IndexedSortedDictionary<float, Entry> entries = [];

    public IntervalMap() { }

    public IntervalMap(IEnumerable<(Interval, T)> input)
    {
        foreach (var (interval, value) in input) Set(interval, value);
    }

    private bool TryGetEntry(float x, out Entry entry) => entries.TryGetLowerBound(x, out _, out entry) && entry.Range.Contains(x);

    private IEnumerable<Entry> GetEntries(Interval range)
    {
        if (TryGetEntry(range.Min, out var entry) && entry.Range.Min < range.Min) yield return entry;
        foreach (var v in entries.GetViewBetween(range.Min, range.Max).Values) yield return v;
    }

    public IEnumerable<(Interval, T)> SubMap(Interval range) => GetEntries(range).Select(e =>
    {
        var interval = e.Range & range;
        return (interval, e.Value);
    });

    public bool TryGetValue(float x, out T value)
    {
        if (!TryGetEntry(x, out var entry))
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            value = default;
            return false;
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        value = entry.Value;
        return true;
    }

    public bool Empty => entries.Empty;

    public void Clear() => entries.Clear();

    public void Clear(Interval range)
    {
        List<Entry> toSet = [];
        List<float> toRemove = [];
        foreach (var entry in GetEntries(range))
        {
            toRemove.Add(entry.Key);

            if (entry.Range.Min < range.Min) toSet.Add(new(new(entry.Range.Min, range.Min), entry.Value));
            if (entry.Range.Max > range.Max) toSet.Add(new(new(range.Max, entry.Range.Max), entry.Value));
        }

        toRemove.ForEach(k => entries.Remove(k));
        toSet.ForEach(e => Set(e.Range, e.Value, coalesce: false));
    }

    public static T DefaultCombiner(T orig, T addend) => addend;

    // Apply a combiner function with a provided addend onto the given range.
    public void Add(Interval range, T value, bool coalesce = true, Func<T, T, T>? combiner = null)
    {
        combiner ??= DefaultCombiner;

        List<Entry> toSet = [];
        foreach (var entry in GetEntries(range))
        {
            if (entry.Range.Min < range.Min) toSet.Add(new(new(entry.Range.Min, range.Min), entry.Value));

            Interval overlap = entry.Range & range;
            if (!overlap.IsEmpty) toSet.Add(new(overlap, combiner(entry.Value, value)));

            if (entry.Range.Max > range.Max) toSet.Add(new(new(range.Max, entry.Range.Max), entry.Value));
        }

        Set(range, value, coalesce);
        toSet.ForEach(e => Set(e.Range, e.Value, coalesce));
    }

    // Explicitly set the contents of a specific range, ignoring previous contents.
    public void Set(Interval range, T value, bool coalesce = true)
    {
        // Coalesce
        bool haveLower = TryGetEntry(range.Min, out var lowerEntry);
        bool haveUpper = TryGetEntry(range.Max, out var upperEntry);
        if (haveLower)
        {
            if (coalesce && EqualityComparer<T>.Default.Equals(lowerEntry!.Value, value))
            {
                range = new(lowerEntry.Range.Min, range.Max);
                entries.Remove(lowerEntry.Key);
            }
            else if (lowerEntry!.Range.Min < range.Min)
            {
                Entry clipped = new(new(lowerEntry.Range.Min, range.Min), lowerEntry.Value);
                entries.Add(lowerEntry.Key, clipped);
            }
            else entries.Remove(lowerEntry.Key);
        }
        if (haveUpper)
        {
            if (coalesce && EqualityComparer<T>.Default.Equals(upperEntry!.Value, value))
            {
                range = new(range.Min, upperEntry.Range.Max);
                entries.Remove(upperEntry.Key);
            }
            else if (upperEntry!.Range.Max > range.Max)
            {
                Entry clipped = new(new(range.Max, upperEntry.Range.Max), upperEntry.Value);
                entries.Remove(upperEntry.Key);
                entries.Add(clipped.Key, clipped);
            }
            else entries.Remove(upperEntry.Key);
        }

        Entry entry = new(range, value);
        entries.Add(entry.Key, entry);
    }

    private IEnumerator<(Interval, T)> GetEnumeratorInternal() => entries.Values.Select(v => (v.Range, v.Value)).GetEnumerator();

    public IEnumerator<(Interval, T)> GetEnumerator() => GetEnumeratorInternal();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();

    public bool Equals(IntervalMap<T> other) => entries.Count == other.entries.Count && entries.GetEnumerator().EnumeratorEqual(other.entries.GetEnumerator());
}
