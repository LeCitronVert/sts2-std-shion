using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionDarkArts() : ShionBerserkCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HpLossVar(4),
        new PowerVar<DarkArts>(1),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (!ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            await CreatureCmd.Damage(choiceContext, card.Owner.Creature, card.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, card);
        }
        await PowerCmd.Apply<DarkArts>(choiceContext, card.Owner.Creature, card.DynamicVars["DarkArts"].BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
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