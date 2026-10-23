using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

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
}
