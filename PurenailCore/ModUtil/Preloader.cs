using PurenailCore.SystemUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PurenailCore.ModUtil
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
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
        private Dictionary<string, Dictionary<string, List<PropertyInfo>>> _props;
        private Dictionary<string, Dictionary<string, List<PropertyInfo>>> Props
        {
            get
            {
                if (_props != null) return _props;

                _props = new();
                var props = GetType().GetProperties().Where(p => p.IsDefined(typeof(Preload), false)).ToList();
                foreach (var prop in props)
                {
                    var preload = prop.GetCustomAttribute<Preload>();
                    if (prop.PropertyType != typeof(GameObject))
                    {
                        throw new ArgumentException($"Improper use of [Preload] attribute: Expected GameObject, but got {prop.PropertyType} on {prop.Name}");
                    }
                    _props.GetOrAddNew(preload.SceneName).GetOrAddNew(preload.ObjectName).Add(prop);
                }
                return _props;
            }
        }

        public List<(string, string)> GetPreloadNames()
        {
            List<(string, string)> l = new();
            foreach (var e in Props)
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
            foreach (var e1 in Props)
            {
                foreach (var e2 in e1.Value)
                {
                    var obj = preloadedObjects[e1.Key][e2.Key];
                    e2.Value.ForEach(p => p.SetValue(this, obj));
                }
            }
        }
    }
}
