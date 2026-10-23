using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Add Fresh Ideas into your Hand equal to your Random. Add Conclusion and 3 random Goofball Token Powers into your Draw Pile.
/// Upgraded: everything created is upgraded.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class SlalDescription() : SlalTheEndlessCard(2,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    public const int GoofballPowers = 3;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("FreshIdeaAmount").WithMultiplier((card, _) => SlalMechanicsHelper.GetRandom(card.Owner.Creature)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Powers.Random>(),
        HoverTipFactory.FromCard<SlalFreshIdea>(IsUpgraded),
        HoverTipFactory.FromCard<SlalConclusion>(IsUpgraded),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;

        List<CardModel> ideas = [];
        for (var i = 0; i < SlalMechanicsHelper.GetRandom(card.Owner.Creature); i++)
        {
            ideas.Add(SlalMechanicsHelper.CreateCard<SlalFreshIdea>(card.Owner, IsUpgraded));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(ideas, PileType.Hand, card.Owner);

        List<CardModel> cards = [SlalMechanicsHelper.CreateCard<SlalConclusion>(card.Owner, IsUpgraded)];
        for (var i = 0; i < GoofballPowers; i++)
        {
            cards.Add(SlalMechanicsHelper.CreateGoofballTokenPower(card.Owner, IsUpgraded));
        }
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Draw, card.Owner, CardPilePosition.Random));
    }

    protected override void OnUpgrade()
    {
    }
}
