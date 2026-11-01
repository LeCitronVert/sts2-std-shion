using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Apply X Weak to ALL enemies. Gain Flashback equal to the number of different cards (upgraded: cards) in your Hand. Exhaust.
/// </summary>
public class SlalWaitASecond() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override bool HasEnergyCostX => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("WeakAmount").WithMultiplier((card, _) => PreviewHelper.XValue(card)),
        new CalculatedVar("FlashbackAmount").WithMultiplier((card, _) => CountCards(card, PreviewHelper.OtherCardsInHand(card))),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<Flashback>(),
    ];

    /// <summary>
    /// Cards in the Hand (this one has left it when it resolves): different ones only, unless upgraded.
    /// </summary>
    private static int CountCards(CardModel card, IReadOnlyCollection<CardModel> hand) =>
        card.IsUpgraded ? hand.Count : SlalMechanicsHelper.CountDifferentCards(hand);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var weak = ResolveEnergyXValue();
        await SlalMechanicsHelper.ApplyToAllEnemies<WeakPower>(choiceContext, card.Owner.Creature, weak, card);

        var flashback = CountCards(card, PileType.Hand.GetPile(card.Owner).Cards.ToList());
        await SlalMechanicsHelper.GainFlashback(choiceContext, card.Owner.Creature, flashback, card);
    }

    protected override void OnUpgrade()
    {
    }
}
