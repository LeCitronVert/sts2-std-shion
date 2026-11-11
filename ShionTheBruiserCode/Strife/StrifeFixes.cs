using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Act 4 has no normal or Elite encounters. Empty lists are left out of the run save, so they load back as null and
/// RoomSet.FromSave crashes ("Value cannot be null. (Parameter 'source')"). Treat missing lists as empty.
/// Also fixes saves made before this patch.
/// </summary>
[HarmonyPatch(typeof(RoomSet), nameof(RoomSet.FromSave))]
public static class StrifeRoomSetLoadPatch
{
    [HarmonyPrefix]
    public static void Prefix(SerializableRoomSet save)
    {
        save.EventIds ??= [];
        save.NormalEncounterIds ??= [];
        save.EliteEncounterIds ??= [];
    }
}

/// <summary>
/// The rest site character picks its campfire animation by act number (Overgrowth / Hive / Glory) and throws
/// "Unexpected act" for any other act. In Act 4, it uses Glory's (Act 3) one, like Act 4's rest site art.
/// The act index is swapped back right after _Ready (nothing else runs in between). Visual only.
/// </summary>
[HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter._Ready))]
public static class StrifeRestSiteCharacterPatch
{
    private const int GloryActIndex = 2;

    private static readonly AccessTools.FieldRef<RunState, int> ActIndexRef =
        AccessTools.FieldRefAccess<RunState, int>("_currentActIndex");

    [HarmonyPrefix]
    public static void Prefix(NRestSiteCharacter __instance, out int? __state)
    {
        __state = null;
        if (__instance.Player?.RunState is not RunState state || state.CurrentActIndex <= GloryActIndex) return;
        __state = state.CurrentActIndex;
        ActIndexRef(state) = GloryActIndex;
    }

    [HarmonyFinalizer]
    public static void Finalizer(NRestSiteCharacter __instance, int? __state)
    {
        if (__state is { } index && __instance.Player?.RunState is RunState state) ActIndexRef(state) = index;
    }
}

/// <summary>
/// Multiplayer HP scaling is defined for acts 1-3 only and throws for Act 4 (every creature of the Architect fight).
/// Act 4 uses Act 3's scaling (×1.3 bosses, ×1.2 the rest). Same on every client.
/// </summary>
[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Models.Singleton.MultiplayerScalingModel),
    nameof(MegaCrit.Sts2.Core.Models.Singleton.MultiplayerScalingModel.GetMultiplayerScaling))]
public static class StrifeMultiplayerScalingPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref int actIndex)
    {
        if (actIndex > 2) actIndex = 2;
    }
}

/// <summary>
/// When the last Decimillipede segment dies, the base game fades out and removes the node of EVERY enemy in the fight
/// (it assumes the encounter is only segments). In the Architect's fight that removed him (and Entomancer) too: he kept
/// acting with no node. There, only the segments fade out. Visual only (same logic, filtered list).
/// </summary>
[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Models.Powers.ReattachPower), "DoFadeOutOnAllSegments")]
public static class StrifeDecimillipedeFadeOutPatch
{
    [HarmonyPrefix]
    public static bool Prefix(MegaCrit.Sts2.Core.Models.Powers.ReattachPower __instance)
    {
        if (!StrifeLayout.IsStrifeFight(__instance.Owner) || __instance.Owner.CombatState is not { } combatState) return true;

        var room = MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance;
        var nodes = new List<MegaCrit.Sts2.Core.Nodes.Combat.NCreature>();
        foreach (var enemy in combatState.Enemies.Where(e => e.HasPower<MegaCrit.Sts2.Core.Models.Powers.ReattachPower>()))
        {
            if (room?.GetCreatureNode(enemy) is not { } node) continue;
            node.AnimHideIntent();
            nodes.Add(node);
        }
        if (nodes.Count == 0 || MegaCrit.Sts2.Core.Nodes.Vfx.NMonsterDeathVfx.Create(nodes) is not { } vfx) return false;

        var parent = nodes[0].GetParent();
        parent.AddChildSafely(vfx);
        parent.MoveChildSafely(vfx, nodes[0].GetIndex());
        var deathAnimation = MegaCrit.Sts2.Core.Helpers.TaskHelper.RunSafely(PlayAndRemove(vfx, nodes));
        foreach (var node in nodes)
        {
            node.DeathAnimationTask = deathAnimation;
            room?.RemoveCreatureNode(node);
        }
        return false;
    }

    private static async Task PlayAndRemove(MegaCrit.Sts2.Core.Nodes.Vfx.NMonsterDeathVfx vfx, List<MegaCrit.Sts2.Core.Nodes.Combat.NCreature> nodes)
    {
        await MegaCrit.Sts2.Core.Commands.Cmd.Wait(0.25f, ignoreCombatEnd: true);
        await vfx.PlayVfx();
        foreach (var node in nodes) node.QueueFreeSafely();
    }
}

/// <summary>
/// The enemy turn's start takes its list of creatures first, then runs the Before-turn-start hooks and a short wait, and only
/// then clears each one's Block. If one of them is removed from combat in between, clearing its Block throws (its
/// CombatState is null), so the enemy turn never starts and the game hangs. In the Architect's fight, damage dealt to
/// him at that point (e.g. Confusion) is spread to his summons by Field Experiment and can kill one.
/// A creature that is no longer in combat skips its turn start. Same on every client.
/// </summary>
[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Entities.Creatures.Creature), nameof(MegaCrit.Sts2.Core.Entities.Creatures.Creature.AfterTurnStart))]
public static class RemovedCreatureTurnStartPatch
{
    [HarmonyPrefix]
    public static bool Prefix(MegaCrit.Sts2.Core.Entities.Creatures.Creature __instance, ref Task __result)
    {
        if (__instance.CombatState != null) return true;
        __result = Task.CompletedTask;
        return false;
    }
}
