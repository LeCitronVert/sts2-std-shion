using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalProofReading() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalMaybeLater>(IsUpgraded),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var cards = await SlalMechanicsHelper.PutRandomColoredCardsFromDiscardIntoHand(card.Owner, 1);
        foreach (var chosenCard in cards)
        {
            chosenCard.SetToFreeThisTurn();
            if (IsUpgraded && chosenCard.IsUpgradable)
            {
                CardCmd.Upgrade(chosenCard);
            }
        }

        await CardPileCmd.AddGeneratedCardToCombat(SlalMechanicsHelper.CreateCard<SlalMaybeLater>(card.Owner, IsUpgraded), PileType.Hand, card.Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
