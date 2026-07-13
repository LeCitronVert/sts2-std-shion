using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionSpiritBreak() : ShionBerserkCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<SpiritBreak>(3)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ShionMechanicsHelper.EnsureCreatureHasCritTracker(card.Owner.Creature);
        var critAmount = DynamicVars["SpiritBreak"].BaseValue;
        if (ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            critAmount *= 2;
        }
        
        await PowerCmd.Apply<CriticalAttackNext>(
            choiceContext,
            card.Owner.Creature,
            critAmount,
            card.Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SpiritBreak"].UpgradeValueBy(2);
    }
}