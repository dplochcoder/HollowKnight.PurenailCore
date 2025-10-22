using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurenailCore.ModUtil;

internal class Dummy : MonoBehaviour
{
    public new void StartCoroutine(IEnumerator coroutine) => base.StartCoroutine(coroutine);
}

internal static class GameObjectExtensions
{
    public static GameObject? Coalesce(this GameObject self) => self == null ? null : self;

    public static void SetParent(this GameObject self, GameObject parent) => self.transform.SetParent(parent.transform);

    public static void Unparent(this GameObject self) => self.transform.parent = null;

    public static GameObject? Parent(this GameObject self) => self.transform.parent?.gameObject;

    public static T? FindParent<T>(this GameObject self) where T : Component
    {
        var obj = self.transform.parent?.gameObject;
        while (obj != null)
        {
            var component = obj.GetComponent<T>();
            if (component != null) return component;

            obj = obj.transform.parent?.gameObject;
        }
        return null;
    }

    public static T GetOrAddComponent<T>(this GameObject self) where T : Component => self.GetComponent<T>() ?? self.AddComponent<T>();

    public static IEnumerable<T> GetComponentsInChildren<T>(this Scene self, bool inactive = false) where T : Component
    {
        foreach (var obj in self.GetRootGameObjects()) foreach (var t in obj.GetComponentsInChildren<T>(inactive)) yield return t;
    }

    public static IEnumerable<T> FindInterfacesRecursive<T>(this GameObject self, bool inactive = false)
    {
        foreach (var component in self.GetComponentsInChildren<Component>(inactive)) if (component is T t) yield return t;
    }

    public static IEnumerable<T> FindInterfacesRecursive<T>(this Scene self, bool inactive = false)
    {
        foreach (var obj in self.GetRootGameObjects()) foreach (var component in obj.FindInterfacesRecursive<T>(inactive)) yield return component;
    }

    public static IEnumerable<T> FindInterfacesInScene<T>(bool inactive = false) => UnityEngine.SceneManagement.SceneManager.GetActiveScene().FindInterfacesRecursive<T>(inactive);

    public static GameObject SharedFindChild(this GameObject self, string name)
    {
        foreach (Transform child in self.transform) if (child.gameObject.name == name) return child.gameObject;
        return null;
    }

    public static IEnumerable<GameObject> Children(this GameObject self)
    {
        foreach (Transform child in self.transform) yield return child.gameObject;
    }

    public static IEnumerable<GameObject> RecursiveChildren(this GameObject self, Func<GameObject, bool> filter = null)
    {
        if (filter != null && !filter(self)) yield break;

        var queue = new Queue<GameObject>();
        queue.Enqueue(self);
        while (queue.Count > 0)
        {
            var obj = queue.Dequeue();
            yield return obj;

            foreach (var child in obj.Children()) if (filter == null || filter(child)) queue.Enqueue(child);
        }
    }

    public static IEnumerable<GameObject> AllGameObjects(this Scene scene, Func<GameObject, bool> filter = null)
    {
        foreach (var root in scene.GetRootGameObjects()) foreach (var obj in root.RecursiveChildren(filter)) yield return obj;
    }

    public static void DoAfter(this GameObject self, Action action, float delay)
    {
        IEnumerator Routine()
        {
            yield return new WaitForSeconds(delay);
            action();
        }
        self.GetOrAddComponent<Dummy>().StartCoroutine(Routine());
    }
}
