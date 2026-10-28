using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Exhaust 1 card. Another player adds a copy of the exhausted card to their Hand, and adds Return Portal to their Hand.
/// Upgraded: the copy is upgraded, and so is Return Portal. Exhaust.
/// </summary>
public class SlalRandomPortal() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalReturnPortal>(IsUpgraded),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target?.Player);
        var receiver = play.Target.Player;

        var exhausted = (await CardSelectCmd.FromHand(
            choiceContext,
            card.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
            null,
            card
        )).FirstOrDefault();
        if (exhausted is null) return;

        await CardCmd.Exhaust(choiceContext, exhausted);

        var copy = SlalMechanicsHelper.CreateCopyForPlayer(exhausted, receiver, IsUpgraded);
        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, receiver);

        var portal = (SlalReturnPortal)SlalMechanicsHelper.CreateCard<SlalReturnPortal>(receiver, IsUpgraded);
        portal.SetOriginalPlayer(card.Owner);
        await CardPileCmd.AddGeneratedCardToCombat(portal, PileType.Hand, card.Owner);
    }
}
