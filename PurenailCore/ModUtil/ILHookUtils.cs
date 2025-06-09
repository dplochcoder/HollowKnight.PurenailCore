using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PurenailCore.ModUtil;

public static class ILHookUtils
{
    private static readonly List<BindingFlags> ALL_BINDING_FLAGS =
    [
        BindingFlags.Public | BindingFlags.Instance,
        BindingFlags.Public | BindingFlags.Static,
        BindingFlags.NonPublic | BindingFlags.Instance,
        BindingFlags.NonPublic | BindingFlags.Static,
    ];

    public static List<ILHook> HookType<T>(ILContext.Manipulator hook)
    {
        List<ILHook> hooks = [];
        foreach (var flags in ALL_BINDING_FLAGS)
        {
            foreach (var method in typeof(T).GetMethods(flags))
            {
                if (method.ContainsGenericParameters || method.GetMethodBody() == null) continue;
                hooks.Add(new(method.GetStateMachineTarget() ?? method, hook));
            }
        }
        return hooks;
    }

    public static ILHook HookOrig<T>(ILContext.Manipulator hook, string name, BindingFlags flags)
    {
        var method = typeof(T).GetMethod($"orig_{name}", flags) ?? typeof(T).GetMethod(name, flags);
        return new(method, hook);
    }

    public static ILHook HookOrigStateMachine<T>(ILContext.Manipulator hook, string name, BindingFlags flags)
    {
        var method = typeof(T).GetMethod($"orig_{name}", flags) ?? typeof(T).GetMethod(name, flags);
        return new(method.GetStateMachineTarget(), hook);
    }

    public static void GotoRange(this ILCursor self, Func<Instruction, bool> first, Func<Instruction, bool> last)
    {
        self.GotoNext(last);
        self.GotoPrev(first);
    }

    public static bool MatchLdfldName(this Instruction instr, string name) => instr.MatchLdfld(out var info) && info.Name == name;
}
