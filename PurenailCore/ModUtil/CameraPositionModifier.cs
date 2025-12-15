using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using PurenailCore.CollectionUtil;
using System.Collections.Generic;
using UnityEngine;

namespace PurenailCore.ModUtil;

public delegate bool ModifyCameraPosition(Vector3 position, out Vector3 updatedPosition);

public enum CameraModifierPhase
{
    // Modify the camera's destination before camera locks and look offsets are applied.
    TARGET_BEFORE_LOCK,
    // Modify the camera's destination after camera locks and look offsets are applied.
    TARGET_AFTER_LOCK,
    // Modify the camera's final position directly.
    FINAL_POSITON,
}

/// <summary>
/// Central utility for modifying the camera's position invisibly from other controllers.
/// </summary>
public static class CameraPositionModifier
{
    private static readonly Dictionary<CameraModifierPhase, SortedMultimap<float, ModifyCameraPosition>> modifiers = new()
    {
        [CameraModifierPhase.TARGET_BEFORE_LOCK] = [],
        [CameraModifierPhase.TARGET_AFTER_LOCK] = [],
        [CameraModifierPhase.FINAL_POSITON] = [],
    };

    public static void AddModifier(CameraModifierPhase phase, float priority, ModifyCameraPosition modifier) => modifiers[phase].Add(priority, modifier);

    public static void RemoveModifier(CameraModifierPhase phase, float priority, ModifyCameraPosition modifier) => modifiers[phase].Remove(priority, modifier);

    private static bool ApplyModifiers(CameraModifierPhase phase, ref Vector3 pos)
    {
        bool modified = false;
        foreach (var (_, collection) in modifiers[phase])
        {
            foreach (var modifier in collection)
            {
                if (!modifier(pos, out var updatedPos))
                    continue;

                pos = updatedPos;
                modified = true;
            }
        }

        return modified;
    }

    private static Vector3? origCameraPos;

    private static void AfterLateUpdate(On.CameraController.orig_LateUpdate orig, CameraController self)
    {
        if (origCameraPos.HasValue)
        {
            self.transform.position = origCameraPos.Value;
            origCameraPos = null;
        }

        var origPos = self.transform.position;
        var newPos = origPos;

        orig(self);
        if (ApplyModifiers(CameraModifierPhase.FINAL_POSITON, ref newPos))
        {
            origCameraPos = origPos;
            self.transform.position = newPos;
        }
    }

    private static void HookCameraControl(ILContext ctx)
    {
        ILCursor cursor = new(ctx);

        cursor.Goto(0);
        cursor.GotoNext(MoveType.Before, i => i.MatchStfld<CameraController>(nameof(CameraController.destination)));
        cursor.EmitDelegate((Vector3 pos) =>
        {
            Vector3 updated = pos;
            return ApplyModifiers(CameraModifierPhase.TARGET_BEFORE_LOCK, ref updated) ? updated : pos;
        });

        cursor.GotoNext(i => i.MatchCall<Vector3>(nameof(Vector3.SmoothDamp)));
        cursor.GotoPrev(MoveType.Before, i => i.MatchLdflda<CameraController>(nameof(CameraController.destination)));
        cursor.EmitDelegate((CameraController self) =>
        {
            Vector3 updated = self.destination;
            if (ApplyModifiers(CameraModifierPhase.TARGET_AFTER_LOCK, ref updated)) self.destination = updated;

            return self;
        });
    }

    private static bool loaded = false;

    private static readonly List<ILHook> hooks = [];

    public static void Load()
    {
        if (loaded) return;
        loaded = true;

        On.CameraController.LateUpdate += AfterLateUpdate;
        hooks.Add(ILHookUtils.HookOrig<CameraController>(HookCameraControl, "LateUpdate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
    }

    static CameraPositionModifier() => Load();
}
