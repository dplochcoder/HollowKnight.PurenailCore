using System.Collections.Generic;
using UnityEngine;

namespace PurenailCore.GOUtil;

public class RandomIntTicker
{
    private readonly int min;
    private readonly int max;
    private int next;

    public RandomIntTicker(int min, int max)
    {
        this.min = min;
        this.max = max;
        this.next = Random.Range(1, Random.Range(min, max + 1) + 1);
    }

    // Each returned int represents the number of ticks that have passed between the current event and the end of this call.
    // So for a non-random ticker that always takes 5 ticks per event, Tick(3) would yield nothing, a subsequent Tick(21) would yield [18, 13, 8, 3], and a tertiary Tick(5) would yield [4].
    public IEnumerable<int> Tick(int ticks)
    {
        while (next <= ticks)
        {
            yield return ticks - next;
            ticks -= next;
            next = Random.Range(min, max + 1);
        }

        next -= ticks;
    }
}

public class RandomFloatTicker
{
    private readonly float min;
    private readonly float max;
    private float next;

    public RandomFloatTicker(float min, float max)
    {
        this.min = min;
        this.max = max;
        this.next = Random.Range(0, Random.Range(min, max));
    }

    public IEnumerable<float> Tick(float ticks)
    {
        while (next <= ticks)
        {
            yield return ticks - next;
            ticks -= next;
            next = Random.Range(min, max);
        }

        next -= ticks;
    }
}
