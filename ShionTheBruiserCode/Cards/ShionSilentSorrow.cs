using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionSilentSorrow() : ShionTheBruiserCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(5),
        new CalculationExtraVar(15),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((model, _) =>  {
                var missingHpRatio = (model.Owner.Creature.MaxHp - model.Owner.Creature.CurrentHp)
                                     / (float)model.Owner.Creature.MaxHp;

                return (decimal) missingHpRatio;
            }
        ),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ShionCardKeywords.Defensive];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.GainBlock(
            card.Owner.Creature,
            new BlockVar(DynamicVars.CalculatedBlock.Calculate(card.Owner.Creature), ValueProp.Move),
            play
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(4);
        DynamicVars.CalculationExtra.UpgradeValueBy(7);
    }
}