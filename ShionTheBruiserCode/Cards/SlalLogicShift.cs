using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain Block equal to (twice) your Random. Play 1 (2) random colored cards from your Discard Pile.
/// </summary>
public class SlalLogicShift() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((card, _) => SlalMechanicsHelper.GetRandom(card.Owner.Creature)),
        new CardsVar(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Powers.Random>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var block = card.DynamicVars.CalculatedBlock.Calculate(play.Target);
        if (block > 0)
        {
            await CreatureCmd.GainBlock(card.Owner.Creature, block, card.DynamicVars.CalculatedBlock.Props, play);
        }

        var toPlay = SlalMechanicsHelper.PickRandomColoredCards(card.Owner, PileType.Discard, card.DynamicVars.Cards.IntValue,
            c => !c.Keywords.Contains(CardKeyword.Unplayable));
        await SlalMechanicsHelper.AutoPlayFromDiscard(choiceContext, card.Owner, toPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationExtra.UpgradeValueBy(1);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
