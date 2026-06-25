using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEnergySurge() : ShionTheBruiserCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<StrengthPower>(1),
        new PowerVar<VigorPower>(4),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["StrengthPower"].BaseValue,
            card.Owner.Creature,
            card)
        ;

        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["VigorPower"].BaseValue,
            card.Owner.Creature,
            card)
        ;
    }

    protected override void OnUpgrade()
    {
        this.EnergyCost.UpgradeBy(-1);
        this.DynamicVars["VigorPower"].UpgradeValueBy(2);
    }
}