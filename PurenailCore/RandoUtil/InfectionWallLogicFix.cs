using RandomizerCore.Logic;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System.Collections.Generic;

namespace PurenailCore.RandoUtil
{
    public static class InfectionWallLogicFix
    {
        public delegate bool Tester(GenerationSettings gs);

        private const string SENTINEL = "InfectionWallLogicFix";

        public static void Setup(Tester tester, float priority)
        {
            RCData.RuntimeLogicOverride.Subscribe(priority, (gs, lmb) => ApplyLogicFix(tester, gs, lmb));
        }

        public static void ApplyLogicFix(Tester tester, GenerationSettings gs, LogicManagerBuilder lmb)
        {
            // Don't apply the fix twice.
            if (!tester(gs) || lmb.LogicLookup.ContainsKey(SENTINEL)) return;
            lmb.AddLogicDef(new(SENTINEL, "TRUE"));

            const string bothSidesProxy = "(Crossroads_06[door1] | Crossroads_06[left1]) + Defeated_False_Knight";

            lmb.DoLogicEdit(new("Crossroads_06[right1]", $"ORIG + (ROOMRANDO | ${bothSidesProxy})"));
            lmb.DoLogicEdit(new("Crossroads_10[left1]", $"ORIG + (ROOMRANDO | ${bothSidesProxy})"));
        }
    }
}
