using PurenailCore.SystemUtil;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PurenailCore.CollectionUtil;

public record Interval
{
    public readonly float Min;
    public readonly float Max;

    public Interval() { }

    public Interval(float min, float max)
    {
        if (min < max)
        {
            Min = min;
            Max = max;
        }
    }

    public float Span => Max - Min;

    public float Mid => (Min + Max) / 2;

    public bool IsEmpty => Min == Max;

    public float Sample(System.Random r) => r.NextFloat(Min, Max);

    public static Interval Intersection(Interval a, Interval b) => new(Mathf.Max(a.Min, b.Min), Mathf.Min(a.Max, b.Max));

    public static Interval operator +(Interval i, float d) => new(i.Min + d, i.Max + d);

    public static Interval operator -(Interval i, float d) => new(i.Min - d, i.Max - d);

    public static Interval operator &(Interval a, Interval b) => Intersection(a, b);

    public static IntervalSet operator |(Interval a, Interval b) => new([a, b]);

    public override string ToString() => IsEmpty ? "[]" : $"[{Min:0.000}, {Max:0.000}]";
}

public class IntervalSet
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

    public bool IsEmpty => intervals.Count == 0;

    public IndexedWeightedSet<Interval> ToWeightedSet()
    {
        IndexedWeightedSet<Interval> ret = new();
        foreach (var i in intervals) ret.Add(i, i.Span);
        return ret;
    }

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
        a.intervals.Select(i => i & b).Where(i => !i.IsEmpty).ForEach(i => ret.intervals.Add(i));
        return ret;
    }

    public static IntervalSet operator |(IntervalSet a, Interval b) => new(a.intervals.Concat([b]));

    public static IntervalSet operator |(IntervalSet a, IntervalSet b) => new(a.intervals.Concat(b.intervals));

    public IEnumerable<Interval> Intervals() => intervals;

    public override string ToString() => $"[{string.Join(", ", intervals.Select(i => i.ToString()))}]";
}