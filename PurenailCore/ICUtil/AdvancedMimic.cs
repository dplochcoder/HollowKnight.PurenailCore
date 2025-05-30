using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Containers;
using ItemChanger.Extensions;
using ItemChanger.Items;
using ItemChanger.Util;
using System.Linq;
using UnityEngine;

namespace PurenailCore.ICUtil;

internal class AdvancedMimicContainer : MimicContainer
{
    internal const string AdvancedMimic = "AdvancedMimic";

    public override string Name => AdvancedMimic;

    public override GameObject GetNewContainer(ContainerInfo info)
    {
        var mimicContainer = MimicUtil.CreateNewMimic(info);

        var mimicItem = info.giveInfo.items.OfType<AdvancedMimic>().FirstOrDefault();
        if (mimicItem != null)
        {
            if (mimicItem.Scale != null) mimicContainer.transform.localScale *= mimicItem.Scale.Value;

            var fsm = mimicContainer.FindChild("Grub Mimic Top")!.FindChild("Grub Mimic 1")!.LocateMyFSM("Grub Mimic");
            if (mimicItem.MaxSpeed != null)
            {
                fsm.GetState("Chase").GetFirstActionOfType<ChaseObjectGround>().speedMax = mimicItem.MaxSpeed;
                fsm.GetState("Cooldown").GetFirstActionOfType<ChaseObjectGround>().speedMax = mimicItem.MaxSpeed;
            }

            if (mimicItem.Acceleration != null)
            {
                fsm.GetState("Chase").GetFirstActionOfType<ChaseObjectGround>().acceleration = mimicItem.Acceleration;
                fsm.GetState("Cooldown").GetFirstActionOfType<ChaseObjectGround>().acceleration = mimicItem.MaxSpeed;
            }

            if (mimicItem.PurenailHP != null) fsm.gameObject.GetComponent<HealthManager>().hp = mimicItem.PurenailHP.Value;
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
