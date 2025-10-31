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
public class Preload : Attribute, IPreload
{
    public readonly string SceneName;
    public readonly string ObjectName;

    public Preload(string sceneName, string objectName)
    {
        this.SceneName = sceneName;
        this.ObjectName = objectName;
    }

    public bool IsPrefab => false;

    string IPreload.SceneName => SceneName;

    string IPreload.ObjectName => ObjectName;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class PrefabPreload : Attribute, IPreload
{
    public readonly string SceneName;
    public readonly string ObjectName;
    public PrefabPreload(string sceneName, string objectName)
    {
        this.SceneName = sceneName;
        this.ObjectName = objectName;
    }

    public bool IsPrefab => true;

    string IPreload.SceneName => SceneName;

    string IPreload.ObjectName => ObjectName;
}

// Mark resources to load from resources.assets with this.
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class ResourcePreload : Attribute
{
    public readonly string Name;
    public ResourcePreload(string name) => this.Name = name;
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

            foreach (var prop in GetType().GetProperties())
            {
                IPreload? preload = prop.GetCustomAttributes(false).Where(attr => attr is IPreload).FirstOrDefault() as IPreload;
                if (preload == null) continue;

                if (!typeof(UnityEngine.Object).IsAssignableFrom(prop.PropertyType))
                    throw new ArgumentException($"Improper use of [Preload] attribute: Type {prop.PropertyType} is not a UnityEngine.Object type");

                var target = _preloadTargets.GetOrAddNew(prop.PropertyType).GetOrAddNew(preload.SceneName).GetOrAddNew(preload.ObjectName);
                target.isPrefab = preload.IsPrefab;
                target.Props.Add(prop);
            }

            foreach (var field in GetType().GetFields())
            {
                IPreload? preload = field.GetCustomAttributes(false).Where(attr => attr is IPreload).FirstOrDefault() as IPreload;
                if (preload == null) continue;

                if (!typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                    throw new ArgumentException($"Improper use of [Preload] attribute: Type {field.FieldType} is not a UnityEngine.Object type");

                var target = _preloadTargets.GetOrAddNew(field.FieldType).GetOrAddNew(preload.SceneName).GetOrAddNew(preload.ObjectName);
                target.isPrefab = preload.IsPrefab;
                target.Fields.Add(field);
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

            foreach (var prop in GetType().GetProperties())
            {
                var attr = prop.GetCustomAttribute<ResourcePreload>();
                if (attr == null) continue;

                if (!typeof(UnityEngine.Object).IsAssignableFrom(prop.PropertyType))
                    throw new ArgumentException($"Improper use of [ResourcePreload] attribute: Type {prop.PropertyType} is not a UnityEngine.Object type");

                var target = _resourceTargets.GetOrAddNew(prop.PropertyType).GetOrAddNew(attr.Name);
                target.Props.Add(prop);
            }

            foreach (var field in GetType().GetFields())
            {
                var attr = field.GetCustomAttribute<ResourcePreload>();
                if (attr == null) continue;

                if (!typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                    throw new ArgumentException($"Improper use of [ResourcePreload] attribute: Type {field.FieldType} is not a UnityEngine.Object type");

                var target = _resourceTargets.GetOrAddNew(field.FieldType).GetOrAddNew(attr.Name);
                target.Fields.Add(field);
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
        return l.ToArray();
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
