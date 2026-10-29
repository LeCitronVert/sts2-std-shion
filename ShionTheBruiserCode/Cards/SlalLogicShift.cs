using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Always Novel. Choose a card in your Discard Pile to put into your Hand. Discard 1 card. Exhaust. (Lose Exhaust)
/// </summary>
public class SlalLogicShift() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        SlalCardKeywords.AlwaysNovel,
        CardKeyword.Exhaust,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;

        var chosen = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            PileType.Discard.GetPile(card.Owner),
            card.Owner,
            new CardSelectorPrefs(card.SelectionScreenPrompt, 1)
        )).FirstOrDefault();
        if (chosen != null)
        {
            await CardPileCmd.Add(chosen, PileType.Hand);
        }

        var discarded = await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            card.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, card.DynamicVars.Cards.IntValue),
            null,
            card
        );
        await CardCmd.Discard(choiceContext, discarded);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
