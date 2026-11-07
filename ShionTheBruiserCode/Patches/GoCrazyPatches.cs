using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// <see cref="GoCrazy"/>: a card you lack the energy for is playable, and playing it spends all your energy
/// and makes you lose HP equal to the energy you're missing.
/// </summary>
public static class GoCrazyPatches
{
    internal static GoCrazy? PowerFor(CardModel card)
    {
        var power = card.Owner?.Creature.GetPower<GoCrazy>();
        return power is not null && power.CanPayFor(card) ? power : null;
    }
}

/// <summary>Only lifts the energy check when energy is the sole reason (stars etc. still apply).</summary>
[HarmonyPatch(typeof(PlayerCombatState), nameof(PlayerCombatState.HasEnoughResourcesFor))]
public static class GoCrazyHasEnoughResourcesPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel card, ref UnplayableReason reason, ref bool __result)
    {
        if (__result || reason != UnplayableReason.EnergyCostTooHigh || GoCrazyPatches.PowerFor(card) is null) return;

        reason = UnplayableReason.None;
        __result = true;
    }
}

/// <summary>
/// The prefix runs before the energy is spent. Spending more than you have just drops you to 0
/// (LoseEnergy clamps), and the power takes the HP in AfterEnergySpent.
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.SpendResources))]
public static class GoCrazySpendResourcesPatch
{
    [HarmonyPrefix]
    public static void Prefix(CardModel __instance)
    {
        var state = __instance.Owner?.PlayerCombatState;
        if (state is null || __instance.CombatState is null) return;

        var missing = __instance.EnergyCost.GetAmountToSpend() - state.Energy;
        if (missing <= 0 || Hook.ShouldPayExcessEnergyCostWithStars(__instance.CombatState, __instance.Owner)) return;

        GoCrazyPatches.PowerFor(__instance)?.PayFor(__instance, missing);
    }
}
