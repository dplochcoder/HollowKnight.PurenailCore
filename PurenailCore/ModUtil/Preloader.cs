using PurenailCore.SystemUtil;
using System.Collections.Generic;
using UnityEngine;

namespace PurenailCore.ModUtil
{
    // Add these as members on a subclass of Preloader. Then, hook Preloader into the related Mod class methods.
    public class PreloadedObject
    {
        public PreloadedObject(Preloader preloader, string sceneName, string objName) => preloader.Add(this, sceneName, objName);

        private GameObject gameObject;

        public void Init(GameObject gameObject) => this.gameObject = gameObject;

        public GameObject Instantiate() => GameObject.Instantiate(gameObject);
    }

    public class Preloader
    {
        private Dictionary<string, Dictionary<string, List<PreloadedObject>>> preloads;

        public void Add(PreloadedObject pObj, string sceneName, string objName) => preloads.GetOrCreateNew(sceneName).GetOrCreateNew(objName).Add(pObj);

        public List<(string, string)> GetPreloadNames()
        {
            List<(string, string)> l = new();
            foreach (var e in preloads)
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
            foreach (var e1 in preloads)
            {
                foreach (var e2 in e1.Value)
                {
                    var obj = preloadedObjects[e1.Key][e2.Key];
                    e2.Value.ForEach(p => p.Init(obj));
                }
            }
        }
    }
}
