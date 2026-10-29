using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Add Conclusion into your Draw Pile. Decrease this card's cost by 1 each time you play a Novel card. Exhaust.
/// Upgraded: Conclusion+ and Retain.
/// Only the Novel cards played while this card is in combat count.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class SlalDescription() : SlalTheEndlessCard(9,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromCard<SlalConclusion>(IsUpgraded),
    ];

    /// <summary>Novel is removed when the card is played, so remember it before the play resolves.</summary>
    private HashSet<CardModel> _novelCardsBeingPlayed = [];

    // MemberwiseClone shares the set with the canonical model, so give each instance its own.
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _novelCardsBeingPlayed = [];
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        await base.BeforeCardPlayed(cardPlay);
        if (cardPlay.Card != this && cardPlay.Card.Owner == Owner && cardPlay.IsFirstInSeries
            && SlalMechanicsHelper.IsNovel(cardPlay.Card))
        {
            _novelCardsBeingPlayed.Add(cardPlay.Card);
        }
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.IsLastInSeries && _novelCardsBeingPlayed.Remove(cardPlay.Card))
        {
            EnergyCost.AddThisCombat(-1, reduceOnly: true);
        }
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
            SlalMechanicsHelper.CreateCard<SlalConclusion>(card.Owner, IsUpgraded), PileType.Draw, card.Owner, CardPilePosition.Random));
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
