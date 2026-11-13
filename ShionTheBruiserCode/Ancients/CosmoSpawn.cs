using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using ShionTheBruiser.ShionTheBruiserCode.Relics;
using ShionTheBruiser.ShionTheBruiserCode.Timeline;

namespace ShionTheBruiser.ShionTheBruiserCode.Ancients;

/// <summary>
/// When Cosmo takes Neow's place at the start of a run. Needs "Warm Embrace" revealed (by anyone in the party),
/// and never with run modifiers (custom/daily runs run their modifiers through Neow).
///  - Solo: every run that follows a run where this character held a Chaos Emerald (won or lost).
///  - Multiplayer: every other run, as tracked by the host's profile.
/// The decision only reads data every client has (the merged unlock state, the host's synced extras),
/// and uses no RNG, so every client puts the same ancient on the map.
/// </summary>
public static class CosmoSpawn
{
    public static bool ShouldReplaceNeow(RunState runState, UnlockState unlockState)
    {
        if (runState.Modifiers.Count > 0) return false;
        if (!EpochProgress.IsRevealed(unlockState, EpochModelId<WarmEmbraceEpoch>())) return false;

        if (runState.Players.Count == 1)
        {
            return CosmoSaveData.HadEmeraldLastRun(runState.Players[0].Character.Id.Entry);
        }

        // The host is the first player of the lobby
        var cosmo = CosmoUnlockExtras.Of(runState.Players[0]).CosmoNext;
        CosmoSaveData.SetMultiplayerCosmoNext(!cosmo);
        return cosmo;
    }

    private static string EpochModelId<T>() where T : MegaCrit.Sts2.Core.Timeline.EpochModel =>
        MegaCrit.Sts2.Core.Timeline.EpochModel.GetId<T>();
}

/// <summary>Swaps Act 1's Neow for Cosmo once the rooms are rolled. Uses no RNG, so later rolls are unchanged.</summary>
#if STS2_BETA
// Beta added a GenerateRooms(RunState) overload that calls this one
[HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms), [typeof(Rng), typeof(UnlockState), typeof(bool), typeof(bool)])]
#else
[HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms))]
#endif
public static class CosmoSpawnPatch
{
    [HarmonyPostfix]
    public static void Postfix(ActModel __instance, UnlockState unlockState)
    {
        try
        {
            if (CosmoAccess.Rooms(__instance) is not { HasAncient: true } rooms || rooms.Ancient is not Neow) return;
            if (CosmoAccess.StateOf(RunManager.Instance) is not { } runState || runState.Acts.Count == 0 || runState.Acts[0] != __instance) return;
            if (!CosmoSpawn.ShouldReplaceNeow(runState, unlockState)) return;

            rooms.Ancient = ModelDb.AncientEvent<Cosmo>();
            MainFile.Logger.Info("Cosmo welcomes the party instead of Neow");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Could not decide between Neow and Cosmo: {ex}");
        }
    }
}

/// <summary>
/// Remembers, per character, whether the local player ended the run (won or lost) holding a Chaos Emerald.
/// </summary>
[HarmonyPatch(typeof(RunManager), nameof(RunManager.OnEnded))]
public static class CosmoRunEndPatch
{
    [HarmonyPrefix]
    public static void Prefix(RunManager __instance)
    {
        try
        {
            if (CosmoAccess.StateOf(__instance) is not { } state || LocalContext.GetMe(state) is not { } me) return;
            CosmoSaveData.SetHadEmeraldLastRun(me.Character.Id.Entry, me.Relics.OfType<ChaosEmeraldRelic>().Any());
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Could not record the Chaos Emeralds held this run: {ex}");
        }
    }
}
