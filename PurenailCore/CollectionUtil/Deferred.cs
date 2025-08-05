using System;
using System.Collections.Generic;

namespace PurenailCore.CollectionUtil;

// A deferred object which may not exist yet.
//
// Actions can be delegated to it immediately or after creation.
public class Deferred<T>
{
    private readonly List<Action<T>> actions = [];
    private T? target;

    public Deferred() { }
    public Deferred(T target) => this.target = target;

    public bool IsPresent => target != null;

    public void Set(T target)
    {
        this.target = target;

        actions.ForEach(a => a(target));
        actions.Clear();
    }

    public void Set(Deferred<T> src) => src.Do(Set);

    public void Do(Action<T> action)
    {
        if (target == null) actions.Add(action);
        else action(target);
    }

    public Deferred<U> Map<U>(Func<T, U> transformer)
    {
        Deferred<U> lazy = new();
        Do(t => lazy.Set(transformer(t)));
        return lazy;
    }
}
