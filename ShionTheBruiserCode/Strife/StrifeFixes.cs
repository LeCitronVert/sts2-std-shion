using HarmonyLib;
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
