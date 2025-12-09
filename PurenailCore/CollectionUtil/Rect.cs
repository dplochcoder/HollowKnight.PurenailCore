using System;
using UnityEngine;

namespace PurenailCore.CollectionUtil;

public class Rect(Vector2 Center, Vector2 Size) : IEquatable<Rect>
{
    public readonly Vector2 Center = Center;
    public readonly Vector2 Size = (Size.x >= 0 && Size.y >= 0) ? Size : throw new ArgumentException($"Invalid size: {Size}");

    public Rect(Bounds bounds) : this(bounds.center, bounds.size) { }

    public Rect(Interval x, Interval y) : this(new Vector2(x.Mid, y.Mid), new Vector2(x.Span, y.Span)) { }

    public float MinX => Center.x - Size.x / 2;
    public float MaxX => Center.x + Size.x / 2;
    public float MinY => Center.y - Size.y / 2;
    public float MaxY => Center.y + Size.y / 2;
    public Vector2 Min => new(MinX, MinY);
    public Vector2 Max => new(MaxX, MaxY);
    public Interval X => new(MinX, MaxX);
    public Interval Y => new(MinY, MaxY);
    public float Width => Size.x;
    public float Height => Size.y;

    public bool IsEmpty => Size.x == 0 || Size.y == 0;

    public bool Contains(float x, float y) => x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
    public bool Contains(Vector2 p) => Contains(p.x, p.y);
    public bool Contains(Rect rect) => X.Contains(rect.X) && Y.Contains(rect.Y);

    public static Rect Intersection(Rect a, Rect b) => new(a.X & b.X, a.Y & b.Y);

    public override bool Equals(object obj) => obj is Rect rect && Equals(rect);

    public override int GetHashCode() => IsEmpty ? 0 : (Center.x.GetHashCode() ^ Center.y.GetHashCode()) ^ (Size.x.GetHashCode() ^ Size.y.GetHashCode());

    public bool Equals(Rect other) => (IsEmpty && other.IsEmpty) || (Center.x == other.Center.x && Center.y == other.Center.y && Size.x == other.Size.x && Size.y == other.Size.y);

    public static Rect operator &(Rect a, Rect b) => Intersection(a, b);

    public static implicit operator UnityEngine.Rect(Rect r) => new()
    {
        center = r.Center,
        size = r.Size
    };

    public static implicit operator Rect(UnityEngine.Rect r) => new(r.center, r.size);
}