using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Put a Novel card from your Discard Pile into your Hand. It costs 1 less this combat. Exhaust. (Upgrade: Retain.)
/// Shows the number of Novel cards in the Discard Pile (other Old Ideas not counted) and glows red when there are none.
/// </summary>
public class SlalOldIdeas() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new EnergyVar(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("NovelCount").WithMultiplier((card, _) => CountNovelInDiscard(card)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
    ];

    protected override bool ShouldGlowRedInternal => CombatState != null && CountNovelInDiscard(this) == 0;

    private static bool IsNovel(CardModel card) => SlalMechanicsHelper.IsNovel(card);

    private static int CountNovelInDiscard(CardModel card)
    {
        if (card.CombatState == null) return 0;
        return PileType.Discard.GetPile(card.Owner).Cards.Count(c => IsNovel(c) && c is not SlalOldIdeas);
    }

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
            IsNovel
        );

        foreach (var chosenCard in chosenCards)
        {
            if (!chosenCard.EnergyCost.CostsX)
            {
                chosenCard.EnergyCost.AddThisCombat(-card.DynamicVars.Energy.IntValue, true);
            }
            await CardPileCmd.Add(chosenCard, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
