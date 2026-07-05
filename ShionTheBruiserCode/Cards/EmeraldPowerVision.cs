using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class EmeraldPowerVision() : ShionTheBruiserCard(0,
    CardType.Power, CardRarity.Token,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<IntangiblePower>(1),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [DreamersKeywords.EmeraldPower];

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
        
        await PowerCmd.Apply<TemporaryArtifact>(
            choiceContext,
            card.Owner.Creature,
            999,
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