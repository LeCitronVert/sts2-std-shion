using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Bloody Execution can only target an enemy it would kill. CardModel.IsValidTarget isn't virtual; it's checked
/// both when aiming (NCardPlay) and when the play action runs (PlayCardAction), so the check is the same on every client.
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.IsValidTarget))]
public static class BloodyExecutionTargetPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, Creature? target, ref bool __result)
    {
        if (!__result || target is null || __instance is not SeelsBloodyExecution execution) return;
        __result = execution.WouldKill(target);
    }
}
