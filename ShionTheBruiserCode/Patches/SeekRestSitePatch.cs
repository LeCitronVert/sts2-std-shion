using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.RestSiteOptions;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Adds the Seek option to every player's rest site (before relic hooks run, so they can still remove it).
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyRestSiteOptions))]
public static class SeekRestSitePatch
{
    public static void Prefix(IRunState runState, Player player, ICollection<RestSiteOption> options)
    {
        if (ChaosEmeraldHelper.CanSeek(player))
        {
            options.Add(new Seek(player));
        }
    }
}
