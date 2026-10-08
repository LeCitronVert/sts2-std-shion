using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalGorillaForm(): SlalTheEndlessCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<StrengthPower>(1),
        new PowerVar<DexterityPower>(1),
        new DynamicVar("RandomMult", 1)
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Form];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<DexterityPower>(
            choiceContext, 
            card.Owner.Creature, 
            -DynamicVars.Dexterity.BaseValue, 
            card.Owner.Creature, 
            card
        );
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, 
            card.Owner.Creature, 
            DynamicVars.Strength.BaseValue, 
            card.Owner.Creature, 
            card
        );
        await PowerCmd.Apply<GorillaForm>(
            choiceContext, 
            card.Owner.Creature, 
            DynamicVars["RandomMult"].BaseValue, 
            card.Owner.Creature, 
            card
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["RandomMult"].UpgradeValueBy(1);
    }
}