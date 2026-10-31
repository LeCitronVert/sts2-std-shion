using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Base for "The first X [cards] you play each turn ..." powers. X is the power's amount.
/// The card is checked before it resolves (Novel is removed when played), and the effect happens after it resolves.
/// Repeated plays of the same card (Echo Form...) count once.
/// </summary>
public abstract class SlalFirstPlaysEachTurnPower : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private class Data
    {
        public int triggersThisTurn;
        public readonly HashSet<CardModel> pending = [];
    }

    protected override object InitInternalData() => new Data();

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner))
        {
            GetInternalData<Data>().triggersThisTurn = 0;
        }
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || !cardPlay.IsFirstInSeries || !ShouldTrigger(cardPlay)) return Task.CompletedTask;

        var data = GetInternalData<Data>();
        if (data.triggersThisTurn >= Amount) return Task.CompletedTask;

        data.triggersThisTurn++;
        data.pending.Add(cardPlay.Card);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!cardPlay.IsLastInSeries || !GetInternalData<Data>().pending.Remove(cardPlay.Card) || Owner.Player is null) return;

        Flash();
        await OnTriggered(choiceContext, cardPlay, Owner.Player);
    }

    protected abstract bool ShouldTrigger(CardPlay cardPlay);

    protected abstract Task OnTriggered(PlayerChoiceContext choiceContext, CardPlay cardPlay, Player player);
}
