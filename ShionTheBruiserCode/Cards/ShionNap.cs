using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionNap() : ShionTheBruiserCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("XBonus", 0)
    ];
    protected override bool HasEnergyCostX => true;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var xAmount = card.ResolveEnergyXValue() + DynamicVars["XBonus"].BaseValue;
        if (xAmount == 0) return;
        await CreatureCmd.Heal(card.Owner.Creature, xAmount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["XBonus"].UpgradeValueBy(1);
    }
}