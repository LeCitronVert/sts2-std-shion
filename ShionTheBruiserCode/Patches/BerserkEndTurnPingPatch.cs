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
using ShionTheBruiser.ShionTheBruiserCode.Strife;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Co-op "hurry up" ping overrides. Display only: reads state, changes nothing.
/// <list type="bullet">
/// <item>Act 4 Architect fight: <c>SHIONTHEBRUISER-STRIFE_PING.{character}.{enemiesUp|architectUp}[Berserk|Dead]</c>
/// from <c>characters.json</c> (<c>{character}</c> = <see cref="StrifeArchitectIntro.ThreatKey"/>). "Enemies up" = one of his
/// summons is alive, otherwise "Architect up". The Berserk variant falls back to the normal one.</item>
/// <item>Otherwise, while the pinging player is alive and Berserk: <c>{CHARACTER}.banter.alive.endTurnPingBerserk</c>.</item>
/// </list>
/// Falls through to vanilla when no key exists (Slal, Fry, other mods' characters…).
/// </summary>
[HarmonyPatch(typeof(FlavorSynchronizer), "CreateEndTurnPingDialogueIfNecessary")]
public static class BerserkEndTurnPingPatch
{
    private const string StrifePingPrefix = "SHIONTHEBRUISER-STRIFE_PING";

    private static readonly AccessTools.FieldRef<FlavorSynchronizer, Dictionary<Player, NSpeechBubbleVfx?>> DialoguesRef =
        AccessTools.FieldRefAccess<FlavorSynchronizer, Dictionary<Player, NSpeechBubbleVfx?>>("_endTurnPingDialogues");

    [HarmonyPrefix]
    public static bool Prefix(FlavorSynchronizer __instance, Player player)
    {
        if (NRun.Instance == null) return true;

        var key = StrifeKey(player) ?? BerserkKey(player);
        if (key == null) return true;

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

    private static string? BerserkKey(Player player)
    {
        if (player.Creature.IsDead || !ShionMechanicsHelper.IsBerserk(player.Creature)) return null;
        var key = player.Character.Id.Entry + ".banter.alive.endTurnPingBerserk";
        return LocString.Exists("characters", key) ? key : null;
    }

    private static string? StrifeKey(Player player)
    {
        var creature = player.Creature;
        if (!StrifeLayout.IsStrifeFight(creature) || creature.CombatState is not { } combat) return null;

        var summonsUp = combat.Enemies.Any(c => c.IsAlive && c.Monster is not StrifeArchitect);
        var baseKey = $"{StrifePingPrefix}.{StrifeArchitectIntro.ThreatKey(player.Character)}.{(summonsUp ? "enemiesUp" : "architectUp")}";

        string[] candidates = creature.IsDead
            ? [baseKey + "Dead"]
            : ShionMechanicsHelper.IsBerserk(creature) ? [baseKey + "Berserk", baseKey] : [baseKey];
        return candidates.FirstOrDefault(k => LocString.Exists("characters", k));
    }
}
