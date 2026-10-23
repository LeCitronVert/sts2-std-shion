using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

[Pool(typeof(TokenCardPool))]
public class EmeraldPowerVision() : ShionTheBruiserCard(0,
    CardType.Power, CardRarity.Token,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<IntangiblePower>(1),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [DreamersKeywords.EmeraldPower];
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<CounterStance>(),
        HoverTipFactory.FromPower<IntangiblePower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<IntangiblePower>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["IntangiblePower"].BaseValue,
            card.Owner.Creature,
            card
        );
        
        await PowerCmd.Apply<CounterStance>(
            choiceContext,
            card.Owner.Creature,
            1,
            card.Owner.Creature,
            card
        );

        await DreamersMechanicsHelper.ExhaustAllEmeraldPowers(choiceContext, play);
;    }

    protected override void OnUpgrade()
    {
        DynamicVars["IntangiblePower"].UpgradeValueBy(1);
    }
}