using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Always Novel. Exhaust up to 3 cards in your Hand. Free Shapeshift for each card Exhausted this way
/// (upgraded: the Goofball Token Powers are upgraded). Exhaust.
/// </summary>
public class SlalEmergencyShapeshift() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.AlwaysNovel, CardKeyword.Exhaust];

    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Shapeshift];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.FreeShapeshift),
        HoverTipFactory.FromKeyword(SlalCardKeywords.GoofballTokenPower),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var targets = (await CardSelectCmd.FromHand(
            choiceContext,
            card.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 0, card.DynamicVars.Cards.IntValue),
            null,
            card
        )).ToList();
        if (targets.Count == 0) return;

        foreach (var target in targets)
        {
            await CardCmd.Exhaust(choiceContext, target);
        }
        await SlalMechanicsHelper.Shapeshift(card.Owner, targets.Count, IsUpgraded, free: true);
    }

    protected override void OnUpgrade()
    {
    }
}
