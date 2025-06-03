using ItemChanger;
using Modding;
using PurenailCore.ICUtil;
using System.Collections.Generic;
using UnityEngine;

namespace PurenailCore;

public class PurenailCore : Mod
{
    public static PurenailCore Instance { get; private set; }

    private static readonly string Version = ModUtil.VersionUtil.ComputeVersion<PurenailCore>();

    public override string GetVersion() => Version;

    public PurenailCore() : base("Purenail Core") { Instance = this; }

    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        PriorityEvents.Setup();

        On.UIManager.StartNewGame += (orig, self, permaDeath, bossRush) =>
        {
            orig(self, permaDeath, bossRush);

            ItemChangerMod.CreateSettingsProfile(false);
            ItemChangerMod.AddPlacements([Finder.GetLocation(LocationNames.Journal_Entry_Charged_Lumafly)!.Wrap().Add(new AdvancedMimic())]);
            ItemChangerMod.AddDeployer(new DamageEnemiesRemoverDeployer() { SceneName = SceneNames.Fungus3_archive_02 });
        };
    }
}
