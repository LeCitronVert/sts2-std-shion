using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Put a Power from your Draw Pile into your Hand. This combat, random cards you create can be Colorless. Exhaust (upgrade: no Exhaust).
/// </summary>
public class SlalMoodShift() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MoodShift>()];

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

        await PowerCmd.Apply<MoodShift>(choiceContext, card.Owner.Creature, 1, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
