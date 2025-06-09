using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using PurenailCore.ModUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PurenailCore.ICUtil;

// A central module for injecting custom Unity scenes into HK.
// Multiple mods that provide custom scenes can coordinate using this module to avoid IL hooking conflicts.
public class SceneLoaderModule : ItemChanger.Modules.Module
{
    // Invoked before `sceneName` is loaded. The interceptor *must* invoke `callback` eventually, and exactly once, though it may be deferred.
    // This allows other mods to asynchronously load scenes from other storage before invoking `callback`.
    //
    // An interceptor with no work to do should simply invoke `callback` immediately, then return.
    public delegate void BeforeSceneLoadHook(string sceneName, Action callback);

    // Invoked when any scene is to be unloaded.
    public delegate void UnloadSceneHook(string prevSceneName, string nextSceneName);

    private List<ILHook> hooks = [];

    public override void Initialize() => hooks.Add(ILHookUtils.HookOrigStateMachine<SceneLoad>(OverrideSceneLoadBeginRoutine, "BeginRoutine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));

    public override void Unload() => hooks.ForEach(h => h.Dispose());

    private void OverrideSceneLoadBeginRoutine(ILContext ctx)
    {
        ILCursor cursor = new(ctx);

        cursor.GotoNext(MoveType.After, i => i.MatchCallvirt<SceneLoad>("RecordBeginTime"));
        cursor.Emit(OpCodes.Ldloc_1);
        cursor.EmitDelegate(BeforeSceneLoad);

        cursor.GotoNext(i => i.MatchCallvirt<SceneLoad>("get_IsFetchAllowed"));
        cursor.Remove();
        cursor.EmitDelegate(BeforeIsFetchAllowed);
    }

    private HashSet<BeforeSceneLoadHook> beforeLoadHooks = [];
    private HashSet<UnloadSceneHook> unloadHooks = [];

    public void AddOnBeforeSceneLoad(BeforeSceneLoadHook hook) => beforeLoadHooks.Add(hook);
    public void RemoveOnBeforeSceneLoad(BeforeSceneLoadHook hook) => beforeLoadHooks.Remove(hook);

    public void AddOnUnloadScene(UnloadSceneHook hook) => unloadHooks.Add(hook);
    public void RemoveOnUnloadScene(UnloadSceneHook hook) => unloadHooks.Remove(hook);

    private int expectedCallbacks;
    private HashSet<int> finishedCallbacks = [];

    private void BeforeSceneLoad(SceneLoad sceneLoad)
    {
        var before = beforeLoadHooks.ToList();
        var unload = unloadHooks.ToList();

        var lastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        var nextSceneName = sceneLoad.TargetSceneName;
        unload.ForEach(h => h(lastSceneName, nextSceneName));

        expectedCallbacks = before.Count;
        finishedCallbacks.Clear();
        for (int i = 0; i < before.Count; i++)
        {
            var copy = i;
            before[i](nextSceneName, () => finishedCallbacks.Add(copy));
        }
    }

    private bool BeforeIsFetchAllowed(SceneLoad sceneLoad) => sceneLoad.IsFetchAllowed && finishedCallbacks.Count >= expectedCallbacks;
}
