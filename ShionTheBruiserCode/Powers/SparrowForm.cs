using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The first X times each turn you create a card, play it.
/// Unplayable cards are skipped and don't use up a trigger.
/// </summary>
public class SparrowForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private class Data
    {
        public int triggersThisTurn;
    }

    protected override object InitInternalData() => new Data();

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner)) GetInternalData<Data>().triggersThisTurn = 0;
        return Task.CompletedTask;
    }

    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (creator == null || creator != Owner.Player || card.Owner != Owner.Player) return;
        if (card.Keywords.Contains(CardKeyword.Unplayable)) return;

        var data = GetInternalData<Data>();
        if (data.triggersThisTurn >= Amount) return;

        // Counted before playing, so cards created by the played card can't go over the limit
        data.triggersThisTurn++;
        Flash();
        // Created cards can ask for a choice (e.g. Creative Break, Fresh Idea+), which a ThrowingPlayerChoiceContext crashes on.
        await CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, null);
    }
}
