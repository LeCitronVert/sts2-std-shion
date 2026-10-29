using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The first X non-free cards you create each turn are free to play (until played).
/// Unplayable and X-cost cards are skipped and don't use up a trigger.
/// The displayed amount counts down the triggers left this turn.
/// Cards that are created already free (Splash, Genie Form) don't use up a trigger.
/// </summary>
public class SparrowForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => Math.Max(0, Amount - GetInternalData<Data>().triggersThisTurn);

    private class Data
    {
        public int triggersThisTurn;
    }

    protected override object InitInternalData() => new Data();

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner))
        {
            GetInternalData<Data>().triggersThisTurn = 0;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (creator == null || creator != Owner.Player || card.Owner != Owner.Player) return Task.CompletedTask;
        if (card.Keywords.Contains(CardKeyword.Unplayable) || card.EnergyCost.CostsX) return Task.CompletedTask;
        if (SlalMechanicsHelper.GetEnergyCost(card) <= 0 && card.GetStarCostWithModifiers() <= 0) return Task.CompletedTask;

        var data = GetInternalData<Data>();
        if (data.triggersThisTurn >= Amount) return Task.CompletedTask;

        data.triggersThisTurn++;
        InvokeDisplayAmountChanged();
        Flash();
        card.EnergyCost.SetUntilPlayed(0);
        card.SetStarCostUntilPlayed(0);
        return Task.CompletedTask;
    }
}
