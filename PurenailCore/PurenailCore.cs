using Modding;
using System;

namespace PurenailCore
{
    public class PurenailCore : Mod
    {
        public static PurenailCore Instance { get; private set; }

        private static readonly string Version = ModUtil.VersionUtil.ComputeVersion<PurenailCore>();

        public override string GetVersion() => Version;

        public PurenailCore() : base("Purenail Core") { Instance = this; }
    }
}
