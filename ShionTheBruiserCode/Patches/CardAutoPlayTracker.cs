using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Remembers whether each card's current play is an auto-play.
/// ModifyCardPlayCount runs before the play is recorded in the combat history (and has no CardPlay),
/// so this is the only way for it to know if the card was played from the Hand.
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
public static class CardAutoPlayTracker
{
    private static readonly ConditionalWeakTable<CardModel, StrongBox<bool>> IsAutoPlay = new();

    public static bool IsBeingAutoPlayed(CardModel card) =>
        IsAutoPlay.TryGetValue(card, out var box) && box.Value;

    [HarmonyPrefix]
    public static void Prefix(CardModel __instance, bool isAutoPlay)
    {
        IsAutoPlay.AddOrUpdate(__instance, new StrongBox<bool>(isAutoPlay));
    }
}
