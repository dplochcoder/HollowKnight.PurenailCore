using PurenailCore.SystemUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PurenailCore.ModUtil;

internal interface IPreload
{
    public string SceneName { get; }
    public string ObjectName { get; }
    public bool IsPrefab { get; }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class Preload(string sceneName, string objectName) : Attribute, IPreload
{
    public readonly string SceneName = sceneName;
    public readonly string ObjectName = objectName;

    public bool IsPrefab => false;

    string IPreload.SceneName => SceneName;

    string IPreload.ObjectName => ObjectName;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class PrefabPreload(string sceneName, string objectName) : Attribute, IPreload
{
    public readonly string SceneName = sceneName;
    public readonly string ObjectName = objectName;

    public bool IsPrefab => true;

    string IPreload.SceneName => SceneName;

    string IPreload.ObjectName => ObjectName;
}

// Mark resources to load from resources.assets with this.
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class ResourcePreload(string name) : Attribute
{
    public readonly string Name = name;
}

// Subclass this class with your own Preloader type.
// Then, add methods like this:
//
//   [Preload("Tutorial_01", "Thingy")]
//   public GameObject thingy;
//
// Make sure your Mod invokes GetPreloadNames(), PreloadSceneHooks(), and Initialize(...) appropriately.
public class Preloader
{
    private class Target
    {
        public bool isPrefab;
        public List<FieldInfo> Fields = [];
        public List<PropertyInfo> Props = [];

        public void SetValue(object obj, UnityEngine.Object value)
        {
            Fields.ForEach(f => f.SetValue(obj, value));
            Props.ForEach(p => p.SetValue(obj, value));
        }
    }

    private Dictionary<Type, Dictionary<string, Dictionary<string, Target>>>? _preloadTargets;

    private Dictionary<Type, Dictionary<string, Dictionary<string, Target>>> PreloadTargets
    {
        get
        {
            if (_preloadTargets != null) return _preloadTargets;

            _preloadTargets = [];

            foreach (var propInfo in GetType().GetProperties())
            {
                if (propInfo.GetCustomAttributes(false).Where(attr => attr is IPreload).FirstOrDefault() is not IPreload preload) continue;

                if (!typeof(UnityEngine.Object).IsAssignableFrom(propInfo.PropertyType))
                    throw new ArgumentException($"Improper use of [Preload] attribute: Type {propInfo.PropertyType} is not a UnityEngine.Object type");

                var target = _preloadTargets.GetOrAddNew(propInfo.PropertyType).GetOrAddNew(preload.SceneName).GetOrAddNew(preload.ObjectName);
                target.isPrefab = preload.IsPrefab;
                target.Props.Add(propInfo);
            }

            foreach (var fieldInfo in GetType().GetFields())
            {
                if (fieldInfo.GetCustomAttributes(false).Where(attr => attr is IPreload).FirstOrDefault() is not IPreload preload) continue;

                if (!typeof(UnityEngine.Object).IsAssignableFrom(fieldInfo.FieldType))
                    throw new ArgumentException($"Improper use of [Preload] attribute: Type {fieldInfo.FieldType} is not a UnityEngine.Object type");

                var target = _preloadTargets.GetOrAddNew(fieldInfo.FieldType).GetOrAddNew(preload.SceneName).GetOrAddNew(preload.ObjectName);
                target.isPrefab = preload.IsPrefab;
                target.Fields.Add(fieldInfo);
            }

            return _preloadTargets;
        }
    }

    private Dictionary<Type, Dictionary<string, Target>>? _resourceTargets;

    private Dictionary<Type, Dictionary<string, Target>> ResourceTargets
    {
        get
        {
            if (_resourceTargets != null) return _resourceTargets;

            _resourceTargets = [];

            foreach (var propInfo in GetType().GetProperties())
            {
                var attr = propInfo.GetCustomAttribute<ResourcePreload>();
                if (attr == null) continue;

                if (!typeof(UnityEngine.Object).IsAssignableFrom(propInfo.PropertyType))
                    throw new ArgumentException($"Improper use of [ResourcePreload] attribute: Type {propInfo.PropertyType} is not a UnityEngine.Object type");

                var target = _resourceTargets.GetOrAddNew(propInfo.PropertyType).GetOrAddNew(attr.Name);
                target.Props.Add(propInfo);
            }

            foreach (var fieldInfo in GetType().GetFields())
            {
                var attr = fieldInfo.GetCustomAttribute<ResourcePreload>();
                if (attr == null) continue;

                if (!typeof(UnityEngine.Object).IsAssignableFrom(fieldInfo.FieldType))
                    throw new ArgumentException($"Improper use of [ResourcePreload] attribute: Type {fieldInfo.FieldType} is not a UnityEngine.Object type");

                var target = _resourceTargets.GetOrAddNew(fieldInfo.FieldType).GetOrAddNew(attr.Name);
                target.Fields.Add(fieldInfo);
            }

            return _resourceTargets;
        }
    }


    public List<(string, string)> GetPreloadNames()
    {
        List<(string, string)> l = [];
        if (!PreloadTargets.TryGetValue(typeof(GameObject), out var dict)) return l;

        foreach (var e1 in dict) foreach (var e2 in e1.Value) if (!e2.Value.isPrefab) l.Add((e1.Key, e2.Key));
        return l;
    }

    public (string, Func<IEnumerator>)[] PreloadSceneHooks()
    {
        List<(string, Func<IEnumerator>)> l = [];
        foreach (var e1 in PreloadTargets)
        {
            var type = e1.Key;
            foreach (var e2 in e1.Value)
            {
                var sceneName = e2.Key;
                List<string> objNames = [];
                foreach (var e3 in e2.Value)
                {
                    var objName = e3.Key;
                    var target = e3.Value;
                    if (target.isPrefab) objNames.Add(objName);
                }

                if (objNames.Count > 0)
                {
                    IEnumerator SaveAssets()
                    {
                        var sceneMap = PreloadTargets[type][sceneName];
                        foreach (var obj in Resources.FindObjectsOfTypeAll(type))
                        {
                            if (sceneMap.TryGetValue(obj.name, out var target))
                            {
                                var prefab = UnityEngine.Object.Instantiate(obj);
                                UnityEngine.Object.DontDestroyOnLoad(prefab);
                                if (prefab is GameObject go) go.SetActive(false);
                                prefab.name = obj.name;
                                target.SetValue(this, prefab);
                            }
                        }
                        yield break;
                    }
                    l.Add((sceneName, SaveAssets));
                }
            }
        }
        return [.. l];
    }

    public void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        if (!PreloadTargets.TryGetValue(typeof(GameObject), out var dict)) return;

        foreach (var e1 in dict)
        {
            foreach (var e2 in e1.Value)
            {
                if (!e2.Value.isPrefab)
                {
                    var obj = preloadedObjects[e1.Key][e2.Key];
                    e2.Value.SetValue(this, obj);
                }
            }
        }

        foreach (var e1 in ResourceTargets)
        {
            foreach (var obj in Resources.FindObjectsOfTypeAll(e1.Key))
            {
                if (e1.Value.TryGetValue(obj.name, out var target)) target.SetValue(this, obj);
            }
        }
    }
}
