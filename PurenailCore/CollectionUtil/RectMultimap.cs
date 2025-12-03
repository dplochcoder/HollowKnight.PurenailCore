using PurenailCore.SystemUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PurenailCore.CollectionUtil;

// Multimap extension of a RectMap.
public class RectMultimap<T> : IEnumerable<(Rect, IReadOnlyCollection<T>)>, IEquatable<RectMultimap<T>>
{
    private readonly RectMap<EquatableHashSet<T>> map = [];

    public RectMultimap() { }

    public RectMultimap(IEnumerable<(Rect, T)> items)
    {
        foreach (var (r, v) in items) Add(r, v);
    }

    public RectMultimap(IEnumerable<(Rect, IEnumerable<T>)> items)
    {
        foreach (var (r, v) in items) Add(r, v);
    }

    private static readonly List<T> empty = [];

    public IReadOnlyCollection<T> Get(float x, float y) => map.TryGetValue(x, y, out var set) ? set : empty;

    public IReadOnlyCollection<T> Get(Vector2 p) => Get(p.x, p.y);

    public bool Contains(float x, float y, T value) => Get(x, y).Contains(value);

    public bool Contains(Vector2 p, T value) => Contains(p.x, p.y, value);

    public IEnumerable<(Rect, IReadOnlyCollection<T>)> SubMultimap(Rect rect) => map.SubMap(rect).Select(p => (p.Item1, (IReadOnlyCollection<T>)p.Item2));

    public void Clear() => map.Clear();

    public void Clear(Rect rect) => map.Clear(rect);

    public void Add(Rect rect, IEnumerable<T> values) => map.Add(rect, [.. values], (orig, addend) =>
    {
        EquatableHashSet<T> modified = [.. orig];
        addend.ForEach(e => modified.Add(e));
        return modified;
    });

    public void Add(Rect rect, T value) => Add(rect, [value]);

    public void Remove(Rect rect, IEnumerable<T> values)
    {
        map.Add(rect, [], (orig, _) =>
        {
            EquatableHashSet<T> modified = [.. orig];
            values.ForEach(e => modified.Remove(e));
            return modified;
        });

        List<Rect> toRemove = [.. map.SubMap(rect).Where(p => p.Item2.Count == 0).Select(p => p.Item1)];
        toRemove.ForEach(map.Clear);
    }

    private IEnumerator<(Rect, IReadOnlyCollection<T>)> GetEnumeratorInternal() => map.Select(p => (p.Item1, (IReadOnlyCollection<T>)p.Item2)).GetEnumerator();

    public IEnumerator<(Rect, IReadOnlyCollection<T>)> GetEnumerator() => GetEnumeratorInternal();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();

    public bool Equals(RectMultimap<T> other) => map.Equals(other.map);
}
