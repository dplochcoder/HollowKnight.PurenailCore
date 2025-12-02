using HutongGames.PlayMaker.Actions;
using PurenailCore.SystemUtil;
using System.Collections.Generic;
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

    public static readonly Interval All = new(float.MinValue, float.MaxValue);

    public float Span => Max - Min;

    public float Mid => (Min == float.MinValue && Max == float.MaxValue) ? 0 : (Min + Max) / 2;

    public bool IsEmpty => Min == Max;

    public bool Contains(float x) => !IsEmpty && x >= Min && x <= Max;

    public float Sample(System.Random? r = null) => r?.NextFloat(Min, Max) ?? Random.Range(Min, Max);

    public static Interval Intersection(Interval a, Interval b) => new(Mathf.Max(a.Min, b.Min), Mathf.Min(a.Max, b.Max));

    public static IntervalSet Negation(Interval a)
    {
        List<Interval> intervals = [];
        if (a.IsEmpty) intervals.Add(All);
        else
        {
            if (a.Min != float.MinValue) intervals.Add(new(float.MinValue, a.Min));
            if (a.Max != float.MaxValue) intervals.Add(new(a.Max, float.MaxValue));
        }
        return new(intervals);
    }

    public static Interval operator +(Interval i, float d) => new(i.Min + d, i.Max + d);

    public static Interval operator -(Interval i, float d) => new(i.Min - d, i.Max - d);

    public static Interval operator &(Interval a, Interval b) => Intersection(a, b);

    public static IntervalSet operator ~(Interval a) => Negation(a);

    public static IntervalSet operator |(Interval a, Interval b) => new([a, b]);

    public static IntervalSet operator -(Interval a, Interval b) => a & ~b;

    public override string ToString() => IsEmpty ? "[]" : $"[{Min:0.000}, {Max:0.000}]";
}