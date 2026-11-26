using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Base class of the Chaos Emerald cards. The Emerald Power keyword: always free to play, always Retained, always Exhaust.
/// Each Chaos Emerald adds its card to the Draw Pile at the start of combat; with Super Form, the card is picked from the
/// Emerald wheel instead (see <see cref="Strife.EmeraldWheel"/>).
/// </summary>
public abstract class EmeraldPowerCard(CardType type, TargetType target) :
    ShionTheBruiserCard(0, type, CardRarity.Token, target)
{
    public abstract ChaosEmeraldColor Color { get; }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [DreamersKeywords.EmeraldPower];
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    // ── Always free ──────────────────────────────────────────────────────────

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this) return false;
        modifiedCost = 0m;
        return true;
    }

    public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this) return false;
        modifiedCost = 0m;
        return true;
    }

    // ── Always Exhaust (Power cards too) ─────────────────────────────────────

    protected override CardLocation GetResultLocationForCardPlay() =>
        IsDupe ? base.GetResultLocationForCardPlay() : new CardLocation(Owner, PileType.Exhaust, CardPilePosition.Bottom);
}

/// <summary>Emerald Powers are always Retained (see <see cref="EmeraldPowerCard"/>).</summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.ShouldRetainThisTurn), MethodType.Getter)]
public static class EmeraldPowerRetainPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref bool __result)
    {
        if (__instance is EmeraldPowerCard) __result = true;
    }
}
