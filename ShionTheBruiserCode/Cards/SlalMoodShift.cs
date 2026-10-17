using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalMoodShift() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var power = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            PileType.Draw.GetPile(card.Owner),
            card.Owner,
            new CardSelectorPrefs(card.SelectionScreenPrompt, 1),
            c => c.Type == CardType.Power
        )).FirstOrDefault();

        if (power != null)
        {
            await CardPileCmd.Add(power, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
