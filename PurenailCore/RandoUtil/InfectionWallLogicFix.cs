﻿using RandomizerCore.Logic;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurenailCore.RandoUtil
{
    public static class InfectionWallLogicFix
    {
        public delegate bool Tester(GenerationSettings gs);

        public const float DEFAULT_PRIORITY = 100f;

        public static bool DefaultTester(GenerationSettings gs) => true;

        public static void Setup(Tester tester, float priority)
        {
            RCData.RuntimeLogicOverride.Subscribe(priority, (gs, lmb) => ApplyLogicFix(tester, gs, lmb));
        }

        private const string Gate1Proxy = "Crossroads_06_Proxy[right1]";
        private const string Gate1 = "Crossroads_06[right1]";
        private const string Gate2Proxy = "Crossroads_10_Proxy[left1]";
        private const string Gate2 = "Crossroads_10[left1]";

        public static void ApplyLogicFix(Tester tester, GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!tester(gs)) return;

            lmb.AddWaypoint(new(Gate1Proxy, lmb.LogicLookup[Gate1].ToInfix()));
            lmb.AddWaypoint(new(Gate2Proxy, lmb.LogicLookup[Gate2].ToInfix()));
            lmb.DoLogicEdit(new(Gate1, "FALSE"));
            lmb.DoLogicEdit(new(Gate2, "FALSE"));

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