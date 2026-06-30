using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionLurkingIntensity() : ShionTheBruiserCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<LurkingIntensity>(1),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<LurkingIntensity>(
            choiceContext, 
            card.Owner.Creature, 
            DynamicVars["LurkingIntensity"].BaseValue, 
            card.Owner.Creature, 
            card
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["LurkingIntensity"].BaseValue += 1;
    }
}