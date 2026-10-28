using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Random;
using ShionTheBruiser.ShionTheBruiserCode.Singletons;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Team Blasts start every combat in a hidden pile instead of the Draw Pile (see <see cref="TeamBlastSingleton"/>).
/// Runs right after the deck is cloned into the Draw Pile, so nothing can draw them or react to them.
/// </summary>
[HarmonyPatch(typeof(Player), nameof(Player.PopulateCombatState))]
public static class TeamBlastHiddenPilePatch
{
    [HarmonyPostfix]
    public static void Postfix(Player __instance, Rng rng, CombatState state)
    {
        TeamBlastSingleton.Instance?.HideTeamBlasts(__instance, state);
    }
}
