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
/// Gain 1 Weak. Discard a card in your Hand. Gain 1 Random if it is playable, and 1 Slippery if it is a Status or unplayable.
/// A playable Status (like Slimed) grants both.
/// </summary>
public class SlalCartoonishMimickery() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(1),
        new PowerVar<Powers.Random>(1),
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
        await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);

        var discarded = (await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            card.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1),
            null,
            card
        )).FirstOrDefault();
        if (discarded == null) return;

        var isPlayable = !discarded.Keywords.Contains(CardKeyword.Unplayable);
        var isStatusOrUnplayable = discarded.Type == CardType.Status || !isPlayable;
        await CardCmd.Discard(choiceContext, discarded);

        if (isPlayable)
        {
            await SlalMechanicsHelper.GainRandom(choiceContext, card.Owner.Creature, card.DynamicVars["Random"].BaseValue, card);
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
