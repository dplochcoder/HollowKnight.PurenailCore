using RandomizerCore.Extensions;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System.Collections.Generic;
using System.Linq;

namespace PurenailCore.RandoUtil;

public static class TransitionInjector
{
    public delegate IEnumerable<(string, TransitionDef)> InjectTransitions(RequestBuilder rb);
    private static HashSet<InjectTransitions> injectors = [];

    // Supply a lazy injector here to integrate with base transition rando.
    public static void AddInjector(InjectTransitions injector) => injectors.Add(injector);

    static TransitionInjector() => RequestBuilder.OnUpdate.Subscribe(2000f, ModifyTransitions);

    private static bool RandomizeTransition(TransitionDef def, TransitionSettings.TransitionMode mode) => mode switch
    {
        TransitionSettings.TransitionMode.None => false,
        TransitionSettings.TransitionMode.MapAreaRandomizer => def.IsMapAreaTransition || def.IsTitledAreaTransition,
        TransitionSettings.TransitionMode.FullAreaRandomizer => def.IsTitledAreaTransition,
        TransitionSettings.TransitionMode.RoomRandomizer => true,
        _ => throw new System.ArgumentException($"Unsupported mode: {mode}")
    };

    private static void ModifyTransitions(RequestBuilder rb)
    {
        List<(string, TransitionDef)> newTransitions = [];
        foreach (var injector in injectors) newTransitions.AddRange(injector(rb));

        bool matching = rb.gs.TransitionSettings.TransitionMatching == TransitionSettings.TransitionMatchingSetting.MatchingDirections
            || rb.gs.TransitionSettings.TransitionMatching == TransitionSettings.TransitionMatchingSetting.MatchingDirectionsAndNoDoorToDoor;
        var dualBuilder = rb.EnumerateTransitionGroups().FirstOrDefault(x => x.label == RBConsts.TwoWayGroup) as SelfDualTransitionGroupBuilder;
        var horizontalBuilder = rb.EnumerateTransitionGroups().FirstOrDefault(x => x.label == RBConsts.InLeftOutRightGroup) as SymmetricTransitionGroupBuilder;
        var verticalBuilder = rb.EnumerateTransitionGroups().FirstOrDefault(x => x.label == RBConsts.InTopOutBotGroup) as SymmetricTransitionGroupBuilder;

        List<string> doors = [];
        foreach (var (name, def) in newTransitions)
        {
            rb.EditTransitionRequest(name, info => info.getTransitionDef = () => def);

            if (RandomizeTransition(def, rb.gs.TransitionSettings.Mode))
            {
                if (!matching) dualBuilder!.Transitions.Add(name);
                else if (def.Direction == TransitionDirection.Door) doors.Add(name);
                else if (def.Direction == TransitionDirection.Right) horizontalBuilder!.Group1.Add(name);
                else if (def.Direction == TransitionDirection.Left) horizontalBuilder!.Group2.Add(name);
                else if (def.Direction == TransitionDirection.Bot) verticalBuilder!.Group1.Add(name);
                else verticalBuilder!.Group2.Add(name);
            }
            else
            {
                rb.AddToVanilla(new(def.VanillaTarget, name));
                rb.EnsureVanillaSourceTransition(name);
            }
        }

        if (matching)
        {
            if (doors.Count > 0)
            {
                rb.rng.PermuteInPlace(doors);
                foreach (var door in doors)
                {
                    switch (horizontalBuilder!.Group2.GetTotal() - horizontalBuilder.Group1.GetTotal())
                    {
                        case > 0:
                            horizontalBuilder.Group1.Add(door);
                            break;
                        case < 0:
                            horizontalBuilder.Group2.Add(door);
                            break;
                        case 0:
                            switch (verticalBuilder!.Group2.GetTotal() - verticalBuilder.Group1.GetTotal())
                            {
                                case > 0:
                                    verticalBuilder.Group1.Add(door);
                                    break;
                                case < 0:
                                    verticalBuilder.Group2.Add(door);
                                    break;
                                case 0:
                                    if (rb.rng.NextBool()) horizontalBuilder.Group1.Add(door);
                                    else horizontalBuilder.Group2.Add(door);
                                    break;
                            }
                            break;
                    }
                }
            }
        }
    }
}
