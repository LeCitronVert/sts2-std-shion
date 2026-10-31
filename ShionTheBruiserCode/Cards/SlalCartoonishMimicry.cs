using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Always Novel. Exhaust a card in your Hand. If it costs no energy (X-cost and Unplayable included), gain 1 Slippery.
/// Otherwise, Mimic equal to its energy cost. Exhaust (upgrade: no Exhaust).
/// </summary>
public class SlalCartoonishMimicry() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.AlwaysNovel, CardKeyword.Exhaust];

    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Mimic];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<SlipperyPower>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SlipperyPower>(),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Mimic),
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

        // Read before the card leaves the Hand
        var cost = exhausted.Keywords.Contains(CardKeyword.Unplayable) ? 0 : SlalMechanicsHelper.GetEnergyCost(exhausted);

        await CardCmd.Exhaust(choiceContext, exhausted);

        if (cost <= 0)
        {
            await PowerCmd.Apply<SlipperyPower>(choiceContext, card.Owner.Creature, card.DynamicVars["SlipperyPower"].BaseValue, card.Owner.Creature, card);
        }
        else
        {
            await SlalMechanicsHelper.Mimic(choiceContext, card.Owner, cost);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
