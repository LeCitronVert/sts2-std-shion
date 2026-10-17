using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionSpiritBreak() : ShionBerserkCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    public const int BerserkCritAttacks = 3;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<SpiritBreak>(1),
        new DynamicVar("BerserkCritAttacks", BerserkCritAttacks),
        new CritDisplayTooltipVar()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await ShionMechanicsHelper.EnsureCreatureHasCritTracker(card.Owner.Creature);
        var critAmount = DynamicVars["SpiritBreak"].BaseValue;
        if (ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            critAmount = BerserkCritAttacks;
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
        EnergyCost.UpgradeBy(-1);
    }
}