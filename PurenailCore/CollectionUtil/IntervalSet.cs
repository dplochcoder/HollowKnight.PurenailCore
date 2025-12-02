using PurenailCore.SystemUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PurenailCore.CollectionUtil;

public class IntervalSet : IEquatable<IntervalSet>
{
    private readonly List<Interval> intervals = [];

    public IntervalSet() { }

    public IntervalSet(IEnumerable<Interval> input)
    {
        List<Interval> sorted = [.. input.Where(i => !i.IsEmpty)];
        if (sorted.Count > 0)
        {
            sorted.SortBy(i => i.Min);

            var prev = sorted[0];
            for (int i = 1; i < sorted.Count; i++)
            {
                var next = sorted[i];
                if (prev.Max >= next.Min) prev = new(prev.Min, Mathf.Max(prev.Max, next.Max));
                else
                {
                    intervals.Add(prev);
                    prev = next;
                }
            }
            intervals.Add(prev);
        }
    }

    public static readonly IntervalSet All = new([Interval.All]);

    public bool IsEmpty => intervals.Count == 0;

    public IndexedWeightedSet<Interval> ToWeightedSet()
    {
        IndexedWeightedSet<Interval> ret = new();
        foreach (var i in intervals) ret.Add(i, i.Span);
        return ret;
    }

    public static IntervalSet Negation(IntervalSet a)
    {
        if (a.IsEmpty) return All;

        List<Interval> intervals = [];
        if (a.intervals.First().Min != float.MinValue) intervals.Add(new(float.MinValue, a.intervals.First().Min));
        for (int i = 0; i < a.intervals.Count - 1; i++)
        {
            var b = a.intervals[i].Max;
            var c = a.intervals[i + 1].Min;
            intervals.Add(new(b, c));
        }
        if (a.intervals.Last().Max != float.MaxValue) intervals.Add(new(a.intervals.Last().Max, float.MaxValue));

        return new(intervals);
    }

    public static IntervalSet Intersection(IntervalSet a, IntervalSet b)
    {
        if (a.IsEmpty || b.IsEmpty) return new();

        var iterA = a.intervals.GetEnumerator();
        iterA.MoveNext();
        var iterB = b.intervals.GetEnumerator();
        iterB.MoveNext();

        List<Interval> intervals = [];
        while (true)
        {
            Interval common = iterA.Current & iterB.Current;
            if (!common.IsEmpty) intervals.Add(common);

            if (iterA.Current.Max < iterB.Current.Max)
            {
                if (!iterA.MoveNext()) break;
            }
            else if (!iterB.MoveNext()) break;
        }

        return new(intervals);
    }

    public static IntervalSet operator ~(IntervalSet a) => Negation(a);

    public static IntervalSet operator +(IntervalSet s, float d)
    {
        IntervalSet ret = new();
        s.Intervals().ForEach(i => ret.intervals.Add(i + d));
        return ret;
    }

    public static IntervalSet operator -(IntervalSet s, float d)
    {
        IntervalSet ret = new();
        s.Intervals().ForEach(i => ret.intervals.Add(i - d));
        return ret;
    }

    public static IntervalSet operator &(IntervalSet a, Interval b)
    {
        IntervalSet ret = new();
        a.intervals.Select(i => i & b).Where(i => !i.IsEmpty).ForEach(ret.intervals.Add);
        return ret;
    }

    public static IntervalSet operator &(Interval a, IntervalSet b) => b & a;

    public static IntervalSet operator &(IntervalSet a, IntervalSet b) => Intersection(a, b);

    public static IntervalSet operator |(IntervalSet a, Interval b) => new(a.intervals.Concat([b]));

    public static IntervalSet operator |(Interval a, IntervalSet b) => b | a;

    public static IntervalSet operator |(IntervalSet a, IntervalSet b) => new(a.intervals.Concat(b.intervals));

    public static IntervalSet operator -(IntervalSet a, IntervalSet b) => a & ~b;

    public IEnumerable<Interval> Intervals() => intervals;

    public override string ToString() => $"[{string.Join(", ", intervals.Select(i => i.ToString()))}]";

    public bool Equals(IntervalSet other) => intervals.GetEnumerator().EnumeratorEqual(other.intervals.GetEnumerator());
}
