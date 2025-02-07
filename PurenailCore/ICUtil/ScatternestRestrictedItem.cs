using ItemChanger;
using System;
using System.Reflection;
using UnityEngine;

namespace PurenailCore.ICUtil;

internal static class ScatternestUtil
{
    private static FieldInfo? indexField;

    internal static bool IsForMe(int scatternestIndex)
    {
        var start = ItemChanger.Internal.Ref.Settings.Start!;
        if (start.GetType().Name == "MultiItemchangerStart")
        {
            indexField ??= start.GetType().GetField("_index", BindingFlags.Instance | BindingFlags.NonPublic);
            int index = (int)indexField.GetValue(start);
            return index == scatternestIndex;
        }
        else return true;
    }
}

internal static class ItemSyncUtil
{
    internal static string NameHeader(int scatternestIndex)
    {
        var nicks = ItemSyncMod.ItemSyncMod.ISSettings.nicknames;
        var playerId = ItemSyncMod.ItemSyncMod.ISSettings.MWPlayerId;

        if (ScatternestUtil.IsForMe(scatternestIndex)) return $"{nicks[playerId]}'s";
        for (int id = 0; id < nicks.Count; id++) if (id != playerId) return $"{nicks[id]}'s";
        return "nobody's";
    }
}

public class ScatternestRestrictedItem : AbstractItem
{
    public AbstractItem? Wrapped;
    public int ScatternestIndex;

    public override void ResolveItem(GiveEventArgs args)
    {
        base.ResolveItem(args);

        args.Item = this;
        UIDef = new PlayerRestrictedUIDef(Wrapped!.GetResolvedUIDef()!, ScatternestIndex);
    }

    public override bool Redundant() => false;

    public override string GetPreferredContainer() => Wrapped!.GetPreferredContainer();

    public override bool GiveEarly(string containerType) => ScatternestUtil.IsForMe(ScatternestIndex) ? Wrapped!.GiveEarly(containerType) : false;

    public override void GiveImmediate(GiveInfo info)
    {
        if (ScatternestUtil.IsForMe(ScatternestIndex)) Wrapped!.GiveImmediate(info);
    }
}

internal class PlayerRestrictedUIDef : UIDef
{
    public UIDef? Wrapped;
    public int ScatternestIndex;

    public PlayerRestrictedUIDef(UIDef wrapped, int scatternestIndex)
    {
        Wrapped = wrapped;
        ScatternestIndex = scatternestIndex;
    }

    // For json.
    PlayerRestrictedUIDef() { }

    public override string GetPostviewName() => $"{ItemSyncUtil.NameHeader(ScatternestIndex)} {Wrapped!.GetPostviewName()}";

    public override string GetPreviewName() => $"{ItemSyncUtil.NameHeader(ScatternestIndex)} {Wrapped!.GetPreviewName()}";

    public override string? GetShopDesc() => ScatternestUtil.IsForMe(ScatternestIndex) ? Wrapped!.GetShopDesc() : "Ah, that's rather nice-looking, isn't it?  Too bad you can't use it.";

    public override Sprite GetSprite() => Wrapped!.GetSprite();

    public override void SendMessage(MessageType type, Action? callback = null) => Wrapped!.SendMessage(type, callback);
}
