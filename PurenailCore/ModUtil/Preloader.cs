using PurenailCore.SystemUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PurenailCore.ModUtil
{
    public interface IPreload
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

    // Subclass this class with your own Preloader type.
    // Then, add methods like this:
    //
    //   [Preload("Tutorial_01", "Thingy")]
    //   public GameObject thingy;
    //
    // Make sure your Mod invokes GetPreloadNames() and Initialize(...) appropriately.
    public class Preloader
    {
        private class Target
        {
            public bool isPrefab;
            public List<FieldInfo> Fields = new();
            public List<PropertyInfo> Props = new();

            public void SetValue(object obj, GameObject value)
            {
                Fields.ForEach(f => f.SetValue(obj, value));
                Props.ForEach(p => p.SetValue(obj, value));
            }
        }

        private Dictionary<string, Dictionary<string, Target>> _targets;

        private Dictionary<string, Dictionary<string, Target>> Targets
        {
            get
            {
                if (_targets != null) return _targets;

                _targets = new();

                var props = GetType().GetProperties()
                    .Where(p => p.IsDefined(typeof(Preload), false) || p.IsDefined(typeof(PrefabPreload), false))
                    .ToList();
                foreach (var prop in props)
                {
                    var preload = (IPreload)prop.GetCustomAttribute<Preload>() ?? (IPreload)prop.GetCustomAttribute<PrefabPreload>();
                    if (prop.PropertyType != typeof(GameObject))
                    {
                        throw new ArgumentException($"Improper use of [Preload] attribute: Expected GameObject, but got {prop.PropertyType} on {prop.Name}");
                    }
                    var target = _targets.GetOrAddNew(preload.SceneName).GetOrAddNew(preload.ObjectName);
                    target.isPrefab = preload.IsPrefab;
                    target.Props.Add(prop);
                }

                var fields = GetType().GetFields()
                    .Where(f => f.IsDefined(typeof(Preload), false) || f.IsDefined(typeof(PrefabPreload), false))
                    .ToList();
                foreach (var field in fields)
                {
                    var preload = (IPreload)field.GetCustomAttribute<Preload>() ?? (IPreload)field.GetCustomAttribute<PrefabPreload>();
                    if (field.FieldType != typeof(GameObject))
                    {
                        throw new ArgumentException($"Improper use of [Preload] attribute: Expected GameObject, but got {field.FieldType} on {field.Name}");
                    }
                    var target = _targets.GetOrAddNew(preload.SceneName).GetOrAddNew(preload.ObjectName);
                    target.isPrefab = preload.IsPrefab;
                    target.Fields.Add(field);
                }

                return _targets;
            }
        }

        public List<(string, string)> GetPreloadNames()
        {
            List<(string, string)> l = new();
            foreach (var e in Targets)
            {
                foreach (var e2 in e.Value)
                {
                    if (!e2.Value.isPrefab)
                    {
                        l.Add((e.Key, e2.Key));
                    }
                }
            }
            return l;
        }

        public List<(string, Func<IEnumerator>)> PreloadSceneHooks()
        {
            List<(string, Func<IEnumerator>)> l = new();
            foreach (var e in Targets)
            {
                var sceneName = e.Key;
                List<string> objNames = new();
                foreach (var e2 in e.Value)
                {
                    var objName = e2.Key;
                    var target = e2.Value;
                    if (target.isPrefab)
                    {
                        objNames.Add(objName);
                    }
                }

                if (objNames.Count > 0)
                {
                    IEnumerator SaveAssets()
                    {
                        var sceneMap = Targets[sceneName];
                        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
                        {
                            if (sceneMap.TryGetValue(go.name, out var target))
                            {
                                var prefab = UnityEngine.Object.Instantiate(go);
                                UnityEngine.Object.DontDestroyOnLoad(prefab);
                                prefab.SetActive(false);
                                prefab.name = go.name;
                                target.SetValue(this, prefab);
                            }
                        }
                        yield break;
                    }
                    l.Add((sceneName, SaveAssets));
                }
            }
            return l;
        }

        public void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            foreach (var e1 in Targets)
            {
                foreach (var e2 in e1.Value)
                {
                    var obj = preloadedObjects[e1.Key][e2.Key];
                    e2.Value.SetValue(this, obj);
                }
            }
        }
    }
}
