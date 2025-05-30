using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Containers;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Items;
using ItemChanger.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PurenailCore.ICUtil;

internal class AdvancedMimicContainer : MimicContainer
{
    internal const string AdvancedMimic = "AdvancedMimic";

    public override string Name => AdvancedMimic;

    private static void AddGlobalTransition(PlayMakerFSM fsm, FsmTransition transition)
    {
        var arr = new FsmTransition[fsm.Fsm.GlobalTransitions.Length + 1];
        System.Array.Copy(fsm.Fsm.GlobalTransitions, arr, arr.Length - 1);
        arr[arr.Length - 1] = transition;
        fsm.Fsm.GlobalTransitions = arr;
    }

    public override GameObject GetNewContainer(ContainerInfo info)
    {
        var mimicContainer = MimicUtil.CreateNewMimic(info);

        var mimicItem = info.giveInfo.items.OfType<AdvancedMimic>().FirstOrDefault();
        var fsm = mimicContainer.FindChild("Grub Mimic Top")!.FindChild("Grub Mimic 1")!.LocateMyFSM("Grub Mimic");

        var quickKill = fsm.AddState("Quick Kill");
        quickKill.AddLastAction(new Lambda(() =>
        {
            var hm = fsm.gameObject.GetComponent<HealthManager>();
            hm.ApplyExtraDamage(hm.hp);
        }));
        AddGlobalTransition(fsm, new()
        {
            ToState = "Quick Kill",
            ToFsmState = quickKill,
            FsmEvent = FsmEvent.GetFsmEvent("GRIMM DEFEATED")
        });

        if (mimicItem != null)
        {
            if (mimicItem.Scale != null) mimicContainer.transform.localScale *= mimicItem.Scale.Value;
            if (mimicItem.PurenailHP != null) fsm.gameObject.GetComponent<HealthManager>().hp = mimicItem.PurenailHP.Value;

            List<ChaseObjectGround> chase = [fsm.GetState("Chase").GetFirstActionOfType<ChaseObjectGround>(), fsm.GetState("Cooldown").GetFirstActionOfType<ChaseObjectGround>()];
            if (mimicItem.MaxSpeed != null) chase.ForEach(c => c.speedMax = mimicItem.MaxSpeed.Value);
            if (mimicItem.Acceleration != null) chase.ForEach(c => c.acceleration = mimicItem.Acceleration.Value);
        }

        return mimicContainer;
    }
}

internal class AdvancedMimic : MimicItem
{
    static AdvancedMimic() => Container.DefineContainer<AdvancedMimicContainer>();

    public float? Scale;
    public float? MaxSpeed;
    public float? Acceleration;
    public int? PurenailHP;

    public override string GetPreferredContainer() => AdvancedMimicContainer.AdvancedMimic;

    public override bool GiveEarly(string containerType) => containerType == Container.Mimic || containerType == AdvancedMimicContainer.AdvancedMimic;

    public override void GiveImmediate(GiveInfo info)
    {
        if (info.Container != AdvancedMimicContainer.AdvancedMimic) base.GiveImmediate(info);
    }
}
