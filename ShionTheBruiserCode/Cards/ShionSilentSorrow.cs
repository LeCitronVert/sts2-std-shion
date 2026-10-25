using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionSilentSorrow() : ShionBerserkCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(8),
        new CalculationExtraVar(8),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((model, _) => ShionMechanicsHelper.IsBerserk(model.Owner.Creature) ? 1 : 0),
        new PowerVar<Guard>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Guard>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];
    
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

        if (ShionMechanicsHelper.TriggerBerserk(card.Owner.Creature))
        {
            await PowerCmd.Apply<Guard>(
                choiceContext,
                card.Owner.Creature,
                DynamicVars["Guard"].BaseValue,
                card.Owner.Creature,
                card
            ); 
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Guard"].UpgradeValueBy(1);
    }
}