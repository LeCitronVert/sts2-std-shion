using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEmeraldReflection() : ShionDefensiveCard(2,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(12, ValueProp.Move),
        new EnergyVar(1),
        new PowerVar<DrawCardsNextTurnPower>(1),
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ShionCardKeywords.Defensive];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.GainBlock(
            card.Owner.Creature, 
            card.DynamicVars.Block, 
            play
        );
        await PowerCmd.Apply<EnergyNextTurnPower>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars.Energy.BaseValue,
            card.Owner.Creature,
            card
        );
        await PowerCmd.Apply<DrawCardsNextTurnPower>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["DrawCardsNextTurnPower"].BaseValue,
            card.Owner.Creature,
            card
        );
        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
        DynamicVars["DrawCardsNextTurnPower"].UpgradeValueBy(1);
    }
}