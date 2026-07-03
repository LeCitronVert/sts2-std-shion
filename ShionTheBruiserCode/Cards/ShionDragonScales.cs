using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionDragonScales() : ShionDefensiveCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("BlockAmount").WithMultiplier((model, _) => 
            ShionMechanicsHelper.GetDragonUniqueBuffCount(model.Owner.Creature)
        ),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var blockAmount = ShionMechanicsHelper.GetDragonUniqueBuffCount(card.Owner.Creature) * DynamicVars.CalculationExtra.IntValue;
        if (blockAmount <= 0) return;
        
        await CreatureCmd.GainBlock(
            card.Owner.Creature,
            new BlockVar(blockAmount, ValueProp.Move),
            play,
            false
        );
        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationExtra.UpgradeValueBy(1);
    }
}