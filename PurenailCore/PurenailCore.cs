using Modding;
using PurenailCore.ICUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PurenailCore;

public class PurenailCore : Mod
{
    public static PurenailCore Instance { get; private set; }

    private static readonly string Version = ModUtil.VersionUtil.ComputeVersion<PurenailCore>();

    public override string GetVersion() => Version;

    public PurenailCore() : base("Purenail Core") { Instance = this; }

    public override List<(string, string)> GetPreloadNames() => Preloader.Instance.GetPreloadNames();

    public override (string, Func<IEnumerator>)[] PreloadSceneHooks() => Preloader.Instance.PreloadSceneHooks();

    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Preloader.Instance.Initialize(preloadedObjects);
        PriorityEvents.Setup();
    }
}
