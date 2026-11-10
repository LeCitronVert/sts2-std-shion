using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

/// <summary>
/// Shared calculations for in-combat description previews.
/// </summary>
public static class PreviewHelper
{
    /// <summary>
    /// Cards in the owner's Hand other than this card (it has left the Hand by the time it resolves).
    /// </summary>
    public static List<CardModel> OtherCardsInHand(CardModel card)
    {
        return PileType.Hand.GetPile(card.Owner).Cards.Where(c => c != card).ToList();
    }

    /// <summary>
    /// X this X-cost card would resolve to if played now (all current energy, after X modifiers).
    /// </summary>
    public static int XValue(CardModel card)
    {
        return Hook.ModifyXValue(card.CombatState, card, card.Owner.PlayerCombatState?.Energy ?? 0);
    }

    /// <summary>
    /// Block a single gain of this amount would actually give, after Dexterity and other modifiers.
    /// </summary>
    public static decimal ModifiedBlock(CardModel card, decimal amount, ValueProp props)
    {
        return Hook.ModifyBlock(card.CombatState, card.Owner.Creature, amount, props, card, null, out _);
    }

    /// <summary>
    /// Confusion a single application of this amount by the card would actually give, after Random, Memory
    /// and other modifiers (same calculation as <c>PowerVar&lt;Confusion&gt;</c> previews).
    /// Nothing is applied for 0, so there's no bonus either.
    /// </summary>
    public static decimal ModifiedConfusion(CardModel card, decimal amount, Creature? target = null)
    {
        if (amount <= 0 || card.CombatState is null) return amount;
        return Hook.ModifyPowerAmountGiven(card.CombatState, ModelDb.Power<Confusion>(), card.Owner.Creature, amount, target, card, out _);
    }

    /// <summary>
    /// Amount of power T the target would actually receive if the card applied this amount to it:
    /// the giver's modifiers, then the target's (Artifact blocks a debuff entirely, so this returns 0).
    /// </summary>
    public static decimal ModifiedPowerApplied<T>(CardModel card, decimal amount, Creature target) where T : PowerModel
    {
        if (amount <= 0 || card.CombatState is null) return amount;
        var power = ModelDb.Power<T>();
        var given = Hook.ModifyPowerAmountGiven(card.CombatState, power, card.Owner.Creature, amount, target, card, out _);
        return Hook.ModifyPowerAmountReceived(card.CombatState, power, target, given, card.Owner.Creature, out _);
    }
}
