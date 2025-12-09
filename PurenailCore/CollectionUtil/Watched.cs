using System;
using System.Collections.Generic;

namespace PurenailCore.CollectionUtil;


public delegate void WatchedDelegate<T>(T before, T after);

// Wrapper around a watched value, implementing listeners on value changes.
public class Watched<T>
{
    public event WatchedDelegate<T>? OnChanged;

    private T value;

    public Watched(T value) => this.value = value;

    public Watched(T value, Action<T> onChanged)
    {
        this.value = value;
        OnChanged += (before, after) => onChanged(after);
    }

    public T Get() => value;

    public void Set(T newValue)
    {
        T prev = value;
        value = newValue;
        if (!EqualityComparer<T>.Default.Equals(prev, newValue)) OnChanged?.Invoke(prev, newValue);
    }
}
