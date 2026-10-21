using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionDesperateCharge() : ShionTheBruiserCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<VigorPower>(3),
        new BlockVar(7, ValueProp.Move),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>()
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
    public override bool GainsBlock => true;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            card.Owner.Creature,
            DynamicVars["VigorPower"].BaseValue,
            card.Owner.Creature,
            card
        );
        await CreatureCmd.GainBlock(
            card.Owner.Creature,
            DynamicVars.Block,
            play
        );
        card.EnergyCost.AddThisCombat(1);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VigorPower"].UpgradeValueBy(2);
        DynamicVars.Block.UpgradeValueBy(2);
    }
}