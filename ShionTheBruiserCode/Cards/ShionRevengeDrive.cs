using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionRevengeDrive() : ShionDefensiveCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<VigorPower>(6),
        new PowerVar<RevengeDrive>(4),
    ];

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
        
        await PowerCmd.Apply<RevengeDrive>(
            choiceContext,
            card.Owner.Creature,
            DynamicVars["RevengeDrive"].BaseValue,
            card.Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}