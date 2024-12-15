using ItemChanger;
using System;
using UnityEngine;

namespace PurenailCore.ICUtil;

internal static class ItemSyncUtil
{
    internal static bool IsForMe(int playerId) => ItemSyncMod.ItemSyncMod.ISSettings.MWPlayerId == playerId;

    internal static string NameHeader(int playerId)
    {
        var nicks = ItemSyncMod.ItemSyncMod.ISSettings.nicknames;
        var name = (playerId >= 0 && playerId < nicks.Count) ? nicks[playerId] : "???";
        return $"{name}'s";
    }
}

internal class PlayerRestrictedItem : AbstractItem
{
    public AbstractItem Wrapped;
    public int PlayerId;

    public override void ResolveItem(GiveEventArgs args)
    {
        base.ResolveItem(args);

        args.Item = this;
        UIDef = new PlayerRestrictedUIDef(Wrapped.GetResolvedUIDef(), PlayerId);
    }

    public override bool Redundant() => false;

    public override string GetPreferredContainer() => Wrapped.GetPreferredContainer();

    public override bool GiveEarly(string containerType) => ItemSyncUtil.IsForMe(PlayerId) ? Wrapped.GiveEarly(containerType) : false;

    public override void GiveImmediate(GiveInfo info)
    {
        if (ItemSyncUtil.IsForMe(PlayerId)) Wrapped.GiveImmediate(info);
    }
}

internal class PlayerRestrictedUIDef : UIDef
{
    public UIDef Wrapped;
    public int PlayerId;

    public PlayerRestrictedUIDef(UIDef wrapped, int playerId)
    {
        Wrapped = wrapped;
        PlayerId = playerId;
    }

    // For json.
    PlayerRestrictedUIDef() { }

    public override string GetPostviewName() => $"{ItemSyncUtil.NameHeader(PlayerId)} {Wrapped.GetPostviewName()}";

    public override string GetPreviewName() => $"{ItemSyncUtil.NameHeader(PlayerId)} {Wrapped.GetPreviewName()}";

    public override string? GetShopDesc() => ItemSyncUtil.IsForMe(PlayerId) ? Wrapped.GetShopDesc() : "Ah, that's rather nice-looking, isn't it?  Too bad you can't use it.";

    public override Sprite GetSprite() => Wrapped.GetSprite();

    public override void SendMessage(MessageType type, Action? callback = null) => Wrapped.SendMessage(type, callback);
}
