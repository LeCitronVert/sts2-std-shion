using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalChangeOfPlan() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
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
