using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Thinking Board: cards it gives Novel to get "Neo" in front of their title (for this combat; copies don't keep it).
/// Only cards that don't override Title are covered.
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.Title), MethodType.Getter)]
public static class NeoTitlePatch
{
    private static readonly ConditionalWeakTable<CardModel, object> NeoCards = new();

    public static void MarkAsNeo(CardModel card)
    {
        NeoCards.AddOrUpdate(card, card);
    }

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref string __result)
    {
        if (NeoCards.TryGetValue(__instance, out _))
        {
            __result = "Neo " + __result;
        }
    }
}
