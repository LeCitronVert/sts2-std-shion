using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 (2) Random. Gain Block equal to your Random. Increase this card's cost by 1 this combat.
/// </summary>
public class SlalLogicShift() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public const int CostIncrease = 1;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Powers.Random>(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        // The Random after this card's gain (it shows 1 again after 10)
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((card, _) =>
        {
            var gain = card.DynamicVars["Random"].IntValue;
            var current = SlalMechanicsHelper.GetRandom(card.Owner.Creature);
            return current <= 0 ? gain : (current + gain - 1) % Powers.Random.GetCardAt + 1;
        }),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Powers.Random>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await SlalMechanicsHelper.GainRandom(choiceContext, card.Owner.Creature, card.DynamicVars["Random"].BaseValue, card);

        var block = SlalMechanicsHelper.GetRandom(card.Owner.Creature);
        if (block > 0)
        {
            await CreatureCmd.GainBlock(card.Owner.Creature, block, ValueProp.Move, play);
        }

        card.EnergyCost.AddThisCombat(CostIncrease);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Random"].UpgradeValueBy(1);
    }
}
