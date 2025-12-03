using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace PurenailCore.CollectionUtil;

public class RectMap<T> : IEnumerable<(Rect, T)>
{
    private readonly IntervalMap<IntervalMap<T>> xMap = [];

    public RectMap() { }

    public RectMap(IEnumerable<(Rect, T)> items)
    {
        foreach (var (r, v) in items) Set(r, v);
    }

    public bool TryGetValue(float x, float y, out T value)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        value = default;
#pragma warning restore CS8601 // Possible null reference assignment.

        if (!xMap.TryGetValue(x, out var yMap)) return false;
        return yMap.TryGetValue(y, out value);
    }

    public bool TryGetValue(Vector2 p, out T value) => TryGetValue(p.x, p.y, out value);

    public void Clear() => xMap.Clear();

    public static T DefaultCombiner(T orig, T addend) => addend;

    public void Add(Rect rect, T value, Func<T, T, T>? combiner = null) => xMap.Add(rect.X, new([(rect.Y, value)]), true, (orig, addend) => Merge(orig, addend, combiner ?? DefaultCombiner));

    public void Set(Rect rect, T value) => Add(rect, value);

    private static IntervalMap<T> Merge(IntervalMap<T> orig, IntervalMap<T> addend, Func<T, T, T> combiner)
    {
        IntervalMap<T> modified = new(orig);
        foreach (var (i, v) in addend) modified.Add(i, v, true, combiner);
        return modified;
    }

    private IEnumerator<(Rect, T)> GetEnumeratorInternal()
    {
        foreach (var (x, yMap) in xMap)
        {
            foreach (var (y, v) in yMap)
            {
                yield return (new(x, y), v);
            }
        }
    }

    public IEnumerator<(Rect, T)> GetEnumerator() => GetEnumeratorInternal();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();
}
