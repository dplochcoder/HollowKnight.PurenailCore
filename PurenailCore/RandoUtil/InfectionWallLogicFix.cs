using RandomizerCore.Logic;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace PurenailCore.RandoUtil
{
    public static class InfectionWallLogicFix
    {
        public delegate bool Tester(GenerationSettings gs);

        public const float DEFAULT_PRIORITY = 100f;

        public static void Setup(Tester tester, float priority)
        {
            RCData.RuntimeLogicOverride.Subscribe(priority, (gs, lmb) => ApplyLogicFix(tester, gs, lmb));
        }

        private const string Gate1 = "Crossroads_06[right1]";
        private const string Gate1Proxy = "Crossroads_06_Proxy[right1]";
        private const string Gate2 = "Crossroads_10[left1]";
        private const string Gate2Proxy = "Crossroads_10_Proxy[left1]";

        public static void ApplyLogicFix(Tester tester, GenerationSettings gs, LogicManagerBuilder lmb)
        {
            // Don't apply the fix twice.
            if (!tester(gs) || lmb.LogicLookup.ContainsKey(Gate1Proxy)) return;

            lmb.AddWaypoint(new(Gate1Proxy, $"({lmb.LogicLookup[Gate1].ToInfix()}) + ROOMRANDO"));
            lmb.AddWaypoint(new(Gate2Proxy, $"({lmb.LogicLookup[Gate2].ToInfix()}) + ROOMRANDO"));

            LogicReplacer replacer = new();
            replacer.IgnoredNames.Add(Gate1);
            replacer.IgnoredNames.Add(Gate2);
            replacer.IgnoredNames.Add(Gate1Proxy);
            replacer.IgnoredNames.Add(Gate2Proxy);
            replacer.SimpleTokenReplacements[Gate1] = new(Gate1Proxy);
            replacer.SimpleTokenReplacements[Gate2] = new(Gate2Proxy);
            replacer.Apply(lmb);
        }
    }
}
