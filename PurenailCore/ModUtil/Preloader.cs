using PurenailCore.SystemUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PurenailCore.ModUtil
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class Preload : Attribute
    {
        public readonly string SceneName;
        public readonly string ObjectName;
        
        public Preload(string sceneName, string objectName)
        {
            this.SceneName = sceneName;
            this.ObjectName = objectName;
        }
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

                var props = GetType().GetProperties().Where(p => p.IsDefined(typeof(Preload), false)).ToList();
                foreach (var prop in props)
                {
                    var preload = prop.GetCustomAttribute<Preload>();
                    if (prop.PropertyType != typeof(GameObject))
                    {
                        throw new ArgumentException($"Improper use of [Preload] attribute: Expected GameObject, but got {prop.PropertyType} on {prop.Name}");
                    }
                    _targets.GetOrAddNew(preload.SceneName).GetOrAddNew(preload.ObjectName).Props.Add(prop);
                }

                var fields = GetType().GetFields().Where(f => f.IsDefined(typeof(Preload), false)).ToList();
                foreach (var field in fields)
                {
                    var preload = field.GetCustomAttribute<Preload>();
                    if (field.FieldType != typeof(GameObject))
                    {
                        throw new ArgumentException($"Improper use of [Preload] attribute: Expected GameObject, but got {field.FieldType} on {field.Name}");
                    }
                    _targets.GetOrAddNew(preload.SceneName).GetOrAddNew(preload.ObjectName).Fields.Add(field);
                }

                return _targets;
            }
        }

        public List<(string, string)> GetPreloadNames()
        {
            List<(string, string)> l = new();
            foreach (var e in Targets)
            {
                foreach (var objName in e.Value.Keys)
                {
                    l.Add((e.Key, objName));
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
