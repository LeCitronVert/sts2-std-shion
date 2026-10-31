using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Singletons;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Resets the Team Blast gauge when the party enters a Rest Site (see <see cref="TeamBlastSingleton"/>).
/// The model hook <c>AfterRoomEntered</c> doesn't give access to the run's players, so this patches the static hook.
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterRoomEntered))]
public static class TeamBlastRestSiteResetPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState, AbstractRoom room)
    {
        if (room is not RestSiteRoom) return;
        TeamBlastSingleton.ResetGauge(runState.Players);
    }
}
