using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Colorful Philosophers only rolls from a hardcoded list of the vanilla card pools, so append ours.
/// The event still filters by unlocked pools and the player's own pool, then trims to 3 with its Rng.
/// Option text lives under COLORFUL_PHILOSOPHERS.pages.INITIAL.options.{EnergyColorName uppercased} in events.json.
/// </summary>
[HarmonyPatch(typeof(ColorfulPhilosophers), "CardPoolColorOrder", MethodType.Getter)]
public static class ColorfulPhilosophersPoolsPatch
{
    [HarmonyPostfix]
    public static IEnumerable<CardPoolModel> Postfix(IEnumerable<CardPoolModel> __result) =>
        __result.Concat([
            ModelDb.CardPool<ShionTheBruiserCardPool>(),
            ModelDb.CardPool<SlalTheEndlessCardPool>(),
            ModelDb.CardPool<FryTheCommanderCardPool>(),
            ModelDb.CardPool<SeelsTheRecklessCardPool>()
        ]).ToList();
}
