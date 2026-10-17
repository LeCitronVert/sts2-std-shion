using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Not worded as "Transform" so it can work on Statuses and Curses.
/// </summary>
public class SlalPlotTwist() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        CardModel? toExhaust;
        if (IsUpgraded)
        {
            toExhaust = (await CardSelectCmd.FromHand(
                choiceContext,
                card.Owner,
                new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
                null,
                card
            )).FirstOrDefault();
        }
        else
        {
            toExhaust = SlalMechanicsHelper.ChooseRandomCardWithoutNovelIfPossible(card.Owner, PileType.Hand);
        }

        if (toExhaust != null)
        {
            await CardCmd.Exhaust(choiceContext, toExhaust);
        }

        foreach (var newCard in SlalMechanicsHelper.CreateRandomCards(card.Owner, 1, IsUpgraded))
        {
            await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Hand, card.Owner);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
