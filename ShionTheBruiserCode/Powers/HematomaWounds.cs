using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Hidden. The Wounds Hematoma created this turn are free to play (they lose Unplayable and cost 0).
/// Removed at the end of the turn, which ends the effect.
/// </summary>
public class HematomaWounds : SeelsTheRecklessPower
{
    private class Data
    {
        public readonly HashSet<CardModel> wounds = [];
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    protected override object InitInternalData() => new Data();

    public void AddWounds(IEnumerable<CardModel> wounds) => GetInternalData<Data>().wounds.UnionWith(wounds);

    private bool IsAffected(CardModel card) => GetInternalData<Data>().wounds.Contains(card);

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        return IsAffected(card) && keywords.Remove(CardKeyword.Unplayable);
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!IsAffected(card)) return false;
        modifiedCost = 0;
        return true;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }
}
