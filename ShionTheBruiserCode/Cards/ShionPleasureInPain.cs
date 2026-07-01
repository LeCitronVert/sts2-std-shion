using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionPleasureInPain() : ShionDefensiveCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<PleasureInPain>(30),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<RegenPower>(),
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        ShionCardKeywords.Defensive,
        CardKeyword.Retain,
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<PleasureInPain>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["PleasureInPain"].BaseValue,
            card.Owner.Creature,
            card
        );
        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PleasureInPain"].UpgradeValueBy(10);
    }
}