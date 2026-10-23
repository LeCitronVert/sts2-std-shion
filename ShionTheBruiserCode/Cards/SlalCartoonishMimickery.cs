using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Exhaust a card in your Hand. Gain as much Weak and Random as its energy cost (0 for X-cost),
/// and 1 Slippery if it is a Status or unplayable.
/// </summary>
public class SlalCartoonishMimickery() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<SlipperyPower>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<Powers.Random>(),
        HoverTipFactory.FromPower<SlipperyPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var exhausted = (await CardSelectCmd.FromHand(
            choiceContext,
            card.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
            null,
            card
        )).FirstOrDefault();
        if (exhausted == null) return;

        // Read everything before the card leaves the Hand
        var cost = SlalMechanicsHelper.GetEnergyCost(exhausted);
        var isStatusOrUnplayable = exhausted.Type == CardType.Status || exhausted.Keywords.Contains(CardKeyword.Unplayable);

        await CardCmd.Exhaust(choiceContext, exhausted);

        if (cost > 0)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, cost, card.Owner.Creature, card);
            await SlalMechanicsHelper.GainRandom(choiceContext, card.Owner.Creature, cost, card);
        }
        if (isStatusOrUnplayable)
        {
            await PowerCmd.Apply<SlipperyPower>(choiceContext, card.Owner.Creature, card.DynamicVars["SlipperyPower"].BaseValue, card.Owner.Creature, card);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
