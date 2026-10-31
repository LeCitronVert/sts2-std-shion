using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Co-op "hurry up" ping: while the pinging player is alive and Berserk, uses
/// <c>{CHARACTER}.banter.alive.endTurnPingBerserk</c> from <c>characters.json</c> if it exists.
/// Otherwise falls through to vanilla. Display only — reads state, changes nothing.
/// </summary>
[HarmonyPatch(typeof(FlavorSynchronizer), "CreateEndTurnPingDialogueIfNecessary")]
public static class BerserkEndTurnPingPatch
{
    private static readonly AccessTools.FieldRef<FlavorSynchronizer, Dictionary<Player, NSpeechBubbleVfx?>> DialoguesRef =
        AccessTools.FieldRefAccess<FlavorSynchronizer, Dictionary<Player, NSpeechBubbleVfx?>>("_endTurnPingDialogues");

    [HarmonyPrefix]
    public static bool Prefix(FlavorSynchronizer __instance, Player player)
    {
        if (NRun.Instance == null) return true;
        if (player.Creature.IsDead || !ShionMechanicsHelper.IsBerserk(player.Creature)) return true;

        string key = player.Character.Id.Entry + ".banter.alive.endTurnPingBerserk";
        if (!LocString.Exists("characters", key)) return true;

        var dialogues = DialoguesRef(__instance);
        dialogues.TryGetValue(player, out var previous);
        if (previous != null && GodotObject.IsInstanceValid(previous))
            previous.QueueFreeSafely();

        var bubble = NSpeechBubbleVfx.Create(new LocString("characters", key).GetFormattedText(),
            player.Creature, 1.5, player.Character.SpeechBubbleColor);
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(bubble);
        dialogues[player] = bubble;
        return false;
    }
}
