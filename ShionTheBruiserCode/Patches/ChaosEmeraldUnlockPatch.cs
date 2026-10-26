using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Chaos Emerald sidequest, last step: winning a run with an Emerald unlocks it in the local player's save file.
/// </summary>
[HarmonyPatch(typeof(RunManager), nameof(RunManager.OnEnded))]
public static class ChaosEmeraldUnlockPatch
{
    public static void Prefix(RunManager __instance, bool isVictory)
    {
        if (!isVictory) return;
        try
        {
            var state = Traverse.Create(__instance).Property<RunState?>("State").Value;
            var me = LocalContext.GetMe(state);
            if (me is null) return;

            var colors = me.Relics.OfType<ChaosEmeraldRelic>().Select(r => r.Color).ToList();
            if (colors.Count > 0) ChaosEmeraldHelper.Unlock(colors);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Could not unlock Chaos Emeralds: {ex}");
        }
    }
}
