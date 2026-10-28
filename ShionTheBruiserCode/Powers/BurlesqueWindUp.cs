using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// This turn, whenever you play a card, gain X Vigor and draw X cards, but Attacks have no effect.
/// Attacks are used up (they still go to their result pile and count for this power) but never resolve:
/// their play count is set to 0, so OnPlay and the "card played" hooks don't run for them.
/// The card that applied it doesn't count.
/// </summary>
public class BurlesqueWindUp(): SlalTheEndlessPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    private class Data
    {
        public CardModel? cardToSkip;
    }

    protected override object InitInternalData() => new Data();

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        GetInternalData<Data>().cardToSkip = cardSource;
        return Task.CompletedTask;
    }

    private bool IsCancelledAttack(CardModel card) => card.Owner.Creature == Owner && card.Type == CardType.Attack;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        return IsCancelledAttack(card) ? 0 : playCount;
    }

    // Cancelled Attacks never reach AfterCardPlayed, so they are rewarded here
    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        if (!IsCancelledAttack(card)) return;
        await Trigger(new BlockingPlayerChoiceContext());
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || cardPlay.Card.Type == CardType.Attack) return;

        var data = GetInternalData<Data>();
        if (cardPlay.Card == data.cardToSkip)
        {
            data.cardToSkip = null;
            return;
        }

        await Trigger(choiceContext);
    }

    private async Task Trigger(PlayerChoiceContext choiceContext)
    {
        if (Owner.Player is null) return;

        Flash();
        var amount = Amount;
        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            Owner,
            amount,
            Owner,
            null
        );
        await CardPileCmd.Draw(choiceContext, amount, Owner.Player);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }
}
