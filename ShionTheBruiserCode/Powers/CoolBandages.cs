using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The next X Wounds are free to play (they lose Unplayable) and make ALL enemies lose HP when played.
/// </summary>
public class CoolBandages : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HpLossVar(4M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Wound>()];

    private bool IsAffected(CardModel card) => card.Owner.Creature == Owner && SeelsMechanicsHelper.IsWound(card);

    /// <summary>Keeps the highest HP loss when stacking an upgraded and a non-upgraded copy.</summary>
    public void SetHpLoss(decimal hpLoss)
    {
        if (hpLoss <= DynamicVars.HpLoss.BaseValue) return;
        AddToSecondaryValue(DynamicVars.HpLoss.Name, hpLoss - DynamicVars.HpLoss.BaseValue);
    }

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

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!IsAffected(cardPlay.Card) || Owner.CombatState is null) return;

        Flash();
        await CreatureCmd.Damage(choiceContext, Owner.CombatState.HittableEnemies.ToList(), DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, Owner, null);
        await PowerCmd.Decrement(this);
    }
}
