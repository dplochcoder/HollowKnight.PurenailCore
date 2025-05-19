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

            if (mimicItem.MaxSpeed != null)
            {
                var fsm = mimicContainer.FindChild("Grub Mimic Top")!.FindChild("Grub Mimic 1")!.LocateMyFSM("Grub Mimic");
                fsm.GetState("Chase").GetFirstActionOfType<ChaseObjectGround>().speedMax = mimicItem.MaxSpeed;
                fsm.GetState("Cooldown").GetFirstActionOfType<ChaseObjectGround>().speedMax = mimicItem.MaxSpeed;
            }
        }

        return mimicContainer;
    }
}

internal class AdvancedMimic : MimicItem
{
    static AdvancedMimic() => Container.DefineContainer<AdvancedMimicContainer>();

    public float? Scale;
    public float? MaxSpeed;

    public override string GetPreferredContainer() => AdvancedMimicContainer.AdvancedMimic;

    public override bool GiveEarly(string containerType) => containerType == Container.Mimic || containerType == AdvancedMimicContainer.AdvancedMimic;

    public override void GiveImmediate(GiveInfo info)
    {
        if (info.Container != AdvancedMimicContainer.AdvancedMimic) base.GiveImmediate(info);
    }
}
