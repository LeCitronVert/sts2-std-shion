using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Base for "Whenever you play a Power, ..." powers.
/// Like Storm, the amount is captured before the card is played so the power doesn't trigger on its own card.
/// </summary>
public abstract class SlalOnPowerPlayedPower : SlalTheEndlessPower
{
    private class Data
    {
        public readonly Dictionary<CardModel, int> amountsForPlayedCards = new();
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || !ShouldTrigger(cardPlay.Card))
            return Task.CompletedTask;

        GetInternalData<Data>().amountsForPlayedCards[cardPlay.Card] = Amount;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player) return;
        if (!GetInternalData<Data>().amountsForPlayedCards.Remove(cardPlay.Card, out var amount) || amount <= 0) return;

        Flash();
        await OnPowerPlayed(choiceContext, cardPlay, amount);
    }

    protected virtual bool ShouldTrigger(CardModel card) => card.Type == CardType.Power;

    protected abstract Task OnPowerPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay, int amount);
}
