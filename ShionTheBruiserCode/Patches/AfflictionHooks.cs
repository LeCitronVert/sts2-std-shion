using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

[HarmonyPatch]
public class AfflictionHooks
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AfflictionModel), nameof(AfflictionModel.AfterApplied))]
    public static void AfterAfflictionApplied(AfflictionModel __instance)
    {
        // Get the card being afflicted
        if (!__instance.HasCard)
            return;

        var card = __instance.Card;

        // Find CounterStance on the creature
        foreach (var listener in card.CombatState?.IterateHookListeners() ?? [])
        {
            if (listener is ICardAfflictionListener afflictionListener)
            {
                afflictionListener.OnAfflictionApplied(__instance, card);
            }
        }
    }
}