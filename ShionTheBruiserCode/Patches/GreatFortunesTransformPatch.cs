using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Great Fortunes: "If this card is transformed, gain 150 gold."
/// Every CardCmd.Transform* overload ends up in this one. The original card is gone from its pile before any hook
/// runs, so the gold is paid after the whole transform finishes, inside the same awaited Task (same timing on every client).
/// </summary>
[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Transform), typeof(IEnumerable<CardTransformation>), typeof(Rng), typeof(CardPreviewStyle))]
public static class GreatFortunesTransformPatch
{
    [HarmonyPrefix]
    public static void Prefix(IEnumerable<CardTransformation> transformations, out List<SeelsGreatFortunes>? __state)
    {
        __state = null;
        foreach (var transformation in transformations)
        {
            if (transformation.Original is not SeelsGreatFortunes fortunes) continue;
            (__state ??= []).Add(fortunes);
        }
    }

    [HarmonyPostfix]
    public static void Postfix(ref Task<IEnumerable<CardPileAddResult>> __result, List<SeelsGreatFortunes>? __state)
    {
        if (__state == null) return;
        __result = PayAfter(__result, __state);
    }

    private static async Task<IEnumerable<CardPileAddResult>> PayAfter(Task<IEnumerable<CardPileAddResult>> transform, List<SeelsGreatFortunes> transformed)
    {
        var results = await transform;
        foreach (var card in transformed)
        {
            // Transform bails out early while combat is ending: the card is then still in its pile
            if (card.Pile != null) continue;

            await PlayerCmd.GainGold(card.DynamicVars.Gold.BaseValue, card.Owner);
        }
        return results;
    }
}
