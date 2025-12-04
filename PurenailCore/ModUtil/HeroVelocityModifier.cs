using HutongGames.PlayMaker.Actions;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using PurenailCore.CollectionUtil;
using System.Collections.Generic;
using UnityEngine;

namespace PurenailCore.ModUtil;

public delegate Vector2 ModifyHeroVelocity(Vector2 velocity);

// Utility to modify the player's velocity, whilst keeping the HeroController ignorant of the changes.
// Can be used to implement mechanics like Celeste bumpers, wind, etc.
public static class HeroVelocityModifier
{
    private static readonly SortedMultimap<float, ModifyHeroVelocity> modifiers = [];

    public static void AddModifier(float priority, ModifyHeroVelocity modifier) => modifiers.Add(priority, modifier);

    public static void RemoveModifier(float priority, ModifyHeroVelocity modifier) => modifiers.Remove(priority, modifier);

    private static Vector2 prevDelta;

    private static Vector2 ComputeDelta(Vector2 value)
    {
        Vector2 orig = value;
        foreach (var (_, collection) in modifiers)
        {
            foreach (var modifier in collection)
            {
                value = modifier(value);
            }
        }

        return value - orig;
    }

    private static Vector2 OverrideGetVelocity(Rigidbody2D rb2d) => rb2d.velocity - prevDelta;

    private static void OverrideGetVelocity2d(On.HutongGames.PlayMaker.Actions.GetVelocity2d.orig_DoGetVelocity orig, GetVelocity2d self)
    {
        orig(self);

        var target = self.Fsm.GetOwnerDefaultTarget(self.gameObject);
        if (target == HeroController.instance?.gameObject)
        {
            var value = OverrideGetVelocity(target.GetComponent<Rigidbody2D>());

            self.vector.Value = value;
            self.x.Value = value.x;
            self.y.Value = value.y;
        }
    }

    private static void OverrideSetVelocity(Rigidbody2D rb2d, Vector2 value) => rb2d.velocity = (prevDelta = ComputeDelta(value)) + value;

    private static void OverrideSetVelocity2d(On.HutongGames.PlayMaker.Actions.SetVelocity2d.orig_DoSetVelocity orig, SetVelocity2d self)
    {
        var target = self.Fsm.GetOwnerDefaultTarget(self.gameObject);
        if (target != HeroController.instance?.gameObject)
        {
            orig(self);
            return;
        }

        var rb2d = target.GetComponent<Rigidbody2D>();
        Vector2 newV = OverrideGetVelocity(rb2d);
        if (!self.x.IsNone) newV.x = self.x.Value;
        if (!self.y.IsNone) newV.y = self.y.Value;

        OverrideSetVelocity(rb2d, newV);
    }

    private static void OverrideFixedUpdate(On.HeroController.orig_FixedUpdate orig, HeroController self)
    {
        orig(self);

        var rb2d = self.gameObject.GetComponent<Rigidbody2D>();
        OverrideSetVelocity(rb2d, OverrideGetVelocity(rb2d));
    }

    private static void HookHeroController(ILContext ctx)
    {
        ILCursor cursor = new(ctx);

        cursor.Goto(0);
        do
        {
            if (cursor.Next.MatchCallvirt<Rigidbody2D>("get_velocity")) cursor.Remove().EmitDelegate(OverrideGetVelocity);
            else if (cursor.Next.MatchCallvirt<Rigidbody2D>("set_velocity")) cursor.Remove().EmitDelegate(OverrideSetVelocity);
        } while (cursor.TryGotoNext(i => true));
    }

    private static readonly List<ILHook> hooks = [];

    private static bool loaded = false;

    public static void Load()
    {
        if (loaded) return;
        loaded = true;

        hooks.AddRange(ILHookUtils.HookType<HeroController>(HookHeroController));
        On.HutongGames.PlayMaker.Actions.GetVelocity2d.DoGetVelocity += OverrideGetVelocity2d;
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.DoSetVelocity += OverrideSetVelocity2d;
        On.HeroController.FixedUpdate += OverrideFixedUpdate;
    }

    static HeroVelocityModifier() => Load();
}
