using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain Block and Random equal to the number of cards played this turn (this one included). Memory doesn't play cards this turn.
/// </summary>
public class SlalCartoonyDodge() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("CardsPlayed").WithMultiplier((card, _) => CountCardsPlayedThisTurn(card) + 1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Powers.Random>(),
        HoverTipFactory.FromPower<Memory>(),
    ];

    private static int CountCardsPlayedThisTurn(CardModel card)
    {
        if (card.CombatState == null) return 0;
        return CombatManager.Instance.History.CardPlaysFinished
            .Count(e => e.HappenedThisTurn(card.CombatState) && e.CardPlay.Card.Owner == card.Owner && e.CardPlay.IsFirstInSeries);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        // This play hasn't finished yet, so count it separately
        var amount = CountCardsPlayedThisTurn(card) + 1;

        await CreatureCmd.GainBlock(card.Owner.Creature, amount, ValueProp.Move, play);
        await SlalMechanicsHelper.GainRandom(choiceContext, card.Owner.Creature, amount, card);
        await PowerCmd.Apply<MemoryBlocked>(choiceContext, card.Owner.Creature, 1, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        //todo : upgrade not specified
        EnergyCost.UpgradeBy(-1);
    }
}
