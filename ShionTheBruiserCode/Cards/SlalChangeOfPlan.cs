using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalChangeOfPlan() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.AlwaysNovel];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("CardsAmount").WithMultiplier((card, _) => PreviewHelper.OtherCardsInHand(card).Count),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var hand = PileType.Hand.GetPile(card.Owner).Cards.ToList();
        if (hand.Count == 0) return;

        await CardCmd.Discard(choiceContext, hand);
        var newCards = SlalMechanicsHelper.CreateRandomCards(card.Owner, hand.Count, IsUpgraded);
        await CardPileCmd.AddGeneratedCardsToCombat(newCards, PileType.Hand, card.Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
