using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionOverdrive() : ShionBerserkCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HpLossVar(4),
        new PowerVar<FreeAttackPower>(1),
        new PowerVar<FreeSkillPower>(1),
        new PowerVar<FreePowerPower>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        ..base.ExtraHoverTips,
        HoverTipFactory.FromPower<FreeAttackPower>(),
        HoverTipFactory.FromPower<FreeSkillPower>(),
        HoverTipFactory.FromPower<FreePowerPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ShionVfx.PlayOverdrive(card.Owner.Creature);
        if (!ShionMechanicsHelper.TriggerBerserk(card.Owner.Creature))
        {
            await CreatureCmd.Damage(choiceContext, card.Owner.Creature, card.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, card, play);
        }
        await PowerCmd.Apply<FreeAttackPower>(choiceContext, card.Owner.Creature, card.DynamicVars["FreeAttackPower"].BaseValue, card.Owner.Creature, card);
        await PowerCmd.Apply<FreeSkillPower>(choiceContext, card.Owner.Creature, card.DynamicVars["FreeSkillPower"].BaseValue, card.Owner.Creature, card);
        await PowerCmd.Apply<FreePowerPower>(choiceContext, card.Owner.Creature, card.DynamicVars["FreePowerPower"].BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FreeAttackPower"].UpgradeValueBy(1);
        DynamicVars["FreeSkillPower"].UpgradeValueBy(1);
        DynamicVars["FreePowerPower"].UpgradeValueBy(1);
    }
    
    // Computed rather than stored as a local cost modifier: HP hooks also reach the deck copy outside combat,
    // and modifiers stored there leak into the next combat's copy (and aren't synced, so they desync multiplayer).
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || !ShionMechanicsHelper.IsBerserk(Owner.Creature)) return false;

        modifiedCost = originalCost - 1;
        return true;
    }
}