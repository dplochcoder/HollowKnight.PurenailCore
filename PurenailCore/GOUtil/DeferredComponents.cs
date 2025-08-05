using ItemChanger.Extensions;
using PurenailCore.CollectionUtil;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PurenailCore.GOUtil;

// Registry for components which may or may not have been installed yet.
public class DeferredComponents : MonoBehaviour
{
    private readonly Dictionary<Type, object> deferred = [];

    internal Deferred<T> Get<T>() where T : Component
    {
        if (deferred.TryGetValue(typeof(T), out var value)) return (value as Deferred<T>)!;

        Deferred<T> d = new();
        deferred.Add(typeof(T), d);
        return d;
    }

    internal void Set<T>(T component) where T : Component => Get<T>().Set(component);
}

public static class DeferredComponentsExtensions
{
    internal static Deferred<T> GetDeferredComponent<T>(this GameObject self) where T : Component
    {
        var direct = self.GetComponent<T>();
        if (direct != null) return new(direct);

        return self.GetOrAddComponent<DeferredComponents>().Get<T>();
    }

    internal static T GotOrAddDeferredComponent<T>(this GameObject self) where T : Component
    {
        var direct = self.GetComponent<T>();
        if (direct != null) return direct;

        return self.AddDeferredComponent<T>();
    }

    internal static T AddDeferredComponent<T>(this GameObject self) where T : Component
    {
        var direct = self.AddComponent<T>();
        self.GetComponent<DeferredComponents>()?.Set(direct);
        return direct;
    }
}
