using PurenailCore.SystemUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PurenailCore.CollectionUtil;

// Multimap extension of interval map.
public class IntervalMultimap<T> : IEnumerable<(Interval, IReadOnlyCollection<T>)>, IEquatable<IntervalMultimap<T>>
{
    private readonly IntervalMap<EquatableHashSet<T>> map = [];

    public IntervalMultimap() { }

    public IntervalMultimap(IEnumerable<(Interval, T)> input)
    {
        foreach (var (interval, value) in input) Add(interval, value);
    }

    public IntervalMultimap(IEnumerable<(Interval, IReadOnlyCollection<T>)> input)
    {
        foreach (var (interval, values) in input) Add(interval, values);
    }

    private static readonly List<T> empty = [];

    public IReadOnlyCollection<T> Get(float x) => map.TryGetValue(x, out var set) ? set : empty;

    public bool Contains(float x, T value) => Get(x).Contains(value);

    public IEnumerable<(Interval, IReadOnlyCollection<T>)> SubMultimap(Interval range) => map.SubMap(range).Select(p => (p.Item1, (IReadOnlyCollection<T>)p.Item2));

    public bool Empty => map.Empty;

    public void Clear() => map.Clear();

    public void Clear(Interval range) => map.Clear(range);

    public void Add(Interval range, IEnumerable<T> values) => map.Add(range, [.. values], true, (orig, addend) =>
    {
        EquatableHashSet<T> modified = [.. orig];
        addend.ForEach(e => modified.Add(e));
        return modified;
    });

    public void Add(Interval range, T value) => Add(range, [value]);

    public void Remove(Interval range, IEnumerable<T> values)
    {
        map.Add(range, [], true,
            (orig, _) => {
                EquatableHashSet<T> modified = [.. orig];
                values.ForEach(v => modified.Remove(v));
                return modified;
            });

        List<Interval> toRemove = [.. map.SubMap(range).Where(p => p.Item2.Count == 0).Select(p => p.Item1)];
        toRemove.ForEach(map.Clear);
    }

    public void Remove(Interval range, T value) => Remove(range, [value]);

    private IEnumerator<(Interval, IReadOnlyCollection<T>)> GetEnumeratorInternal() => map.Select(p => (p.Item1, (IReadOnlyCollection<T>)p.Item2)).GetEnumerator();

    public IEnumerator<(Interval, IReadOnlyCollection<T>)> GetEnumerator() => GetEnumeratorInternal();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();

    public bool Equals(IntervalMultimap<T> other) => map.Equals(other.map);
}
