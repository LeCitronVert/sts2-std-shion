using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Choose any number of cards in your Hand to put into [Original Player]'s Discard Pile. Draw 1 (2) card(s). Retain. Exhaust.
/// Given by Trash Disposal; the Original Player is the one who played it.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class SlalThrowGarbage() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new StringVar("OriginalPlayer"),
    ];

    private Player? _originalPlayer;

    public void SetOriginalPlayer(Player player)
    {
        _originalPlayer = player;
        ((StringVar)DynamicVars["OriginalPlayer"]).StringValue = SlalMechanicsHelper.GetPlayerName(player);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var receiver = _originalPlayer;
        var handCount = PileType.Hand.GetPile(card.Owner).Cards.Count;

        if (receiver is { Creature.IsAlive: true } && handCount > 0)
        {
            var chosen = (await CardSelectCmd.FromHand(
                choiceContext,
                card.Owner,
                new CardSelectorPrefs(card.SelectionScreenPrompt, 0, handCount),
                null,
                card
            )).ToList();

            foreach (var garbage in chosen)
            {
                await SlalMechanicsHelper.GiveCardToPlayer(garbage, receiver, PileType.Discard);
            }
        }

        await CardPileCmd.Draw(choiceContext, card.DynamicVars.Cards.IntValue, card.Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
