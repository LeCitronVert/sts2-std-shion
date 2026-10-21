using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Hidden power randomizing the cost of Erratic cards when drawn,
/// and tracking the cards modified with a Keyword or an Enchantment this combat.
/// </summary>
public class SeelsTracker : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.None;
    protected override bool IsVisibleInternal => false;

    public readonly HashSet<CardModel> ModifiedCards = [];

    public void CountModification(CardModel card)
    {
        ModifiedCards.Add(card);
    }

    public override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner.Creature != Owner || !SeelsMechanicsHelper.IsErratic(card)) return;

        await SeelsMechanicsHelper.RandomizeErraticCost(choiceContext, card);
    }
}
