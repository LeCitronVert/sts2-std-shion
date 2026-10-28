using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you play a card from your Hand, play up to X identical cards from your Discard Pile
/// (a card and its upgrade count as identical, upgraded ones first).
/// </summary>
public class TelevisionForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var played = cardPlay.Card;
        if (played.Owner != Owner.Player || cardPlay.IsAutoPlay || !cardPlay.IsLastInSeries) return;

        var copies = PileType.Discard.GetPile(Owner.Player).Cards
            .Where(c => c != played && c.Id == played.Id && !c.Keywords.Contains(CardKeyword.Unplayable))
            .OrderByDescending(c => c.IsUpgraded)
            .Take(Amount)
            .ToList();
        if (copies.Count == 0) return;

        Flash();
        await SlalMechanicsHelper.AutoPlayFromDiscard(choiceContext, Owner.Player, copies);
    }
}
