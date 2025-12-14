using HutongGames.PlayMaker.Actions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PurenailCore.ModUtil;

/// <summary>
/// Common utility for modifying soul orb effects.
/// </summary>
public static class SoulOrbModifier
{
    public static event Action<FlingObjectsFromGlobalPool, SoulOrb>? OnFlingSoulOrb;

    private static void InvokeOnFlingSoulOrb(FlingObjectsFromGlobalPool fsmAction, GameObject obj)
    {
        var orb = obj.GetComponent<SoulOrb>();
        if (orb != null)
            OnFlingSoulOrb?.Invoke(fsmAction, orb);
    }

    private static void HookFlingObjectsFromGlobalPool(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.GotoNext(i => i.MatchCall<RigidBody2dActionBase>("CacheRigidBody2d"));
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc_S, (byte)4);
        cursor.EmitDelegate(InvokeOnFlingSoulOrb);
    }

    public static event Action<SoulOrb>? OnGiveSoul;

    private static void InvokeOnGiveSoul(SoulOrb soulOrb) => OnGiveSoul?.Invoke(soulOrb);

    private static void HookSoulOrbZoom(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.GotoNext(i => i.MatchCallvirt<HeroController>("AddMPCharge"));
        cursor.Emit(OpCodes.Ldloc_1);
        cursor.EmitDelegate(InvokeOnGiveSoul);
    }

    private static readonly List<ILHook> hooks = [];

    private static bool loaded = false;

    public static void Load()
    {
        if (loaded) return;

        loaded = true;
        hooks.Add(new(typeof(SoulOrb).GetMethod("Zoom", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetStateMachineTarget(), HookSoulOrbZoom));
        hooks.Add(new(typeof(FlingObjectsFromGlobalPool).GetMethod("OnEnter", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance), HookFlingObjectsFromGlobalPool));
    }

    static SoulOrbModifier() => Load();
}
