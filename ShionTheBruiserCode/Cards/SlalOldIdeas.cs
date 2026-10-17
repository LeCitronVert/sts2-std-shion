using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalOldIdeas() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new EnergyVar(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromCard<SlalMaybeLater>(IsUpgraded),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var chosenCards = await CardSelectCmd.FromCombatPile(
            choiceContext,
            PileType.Discard.GetPile(card.Owner),
            card.Owner,
            new CardSelectorPrefs(card.SelectionScreenPrompt, card.DynamicVars.Cards.IntValue),
            c => SlalMechanicsHelper.IsColored(c) && c.Keywords.Contains(SlalCardKeywords.Novel)
        );

        foreach (var chosenCard in chosenCards)
        {
            if (!chosenCard.EnergyCost.CostsX)
            {
                chosenCard.EnergyCost.AddThisCombat(-card.DynamicVars.Energy.IntValue, true);
            }
            await CardPileCmd.Add(chosenCard, PileType.Hand);
        }

        await CardPileCmd.AddGeneratedCardToCombat(SlalMechanicsHelper.CreateCard<SlalMaybeLater>(card.Owner, IsUpgraded), PileType.Hand, card.Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
