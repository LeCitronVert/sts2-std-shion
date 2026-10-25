using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.Commands;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Put 1 card from your Hand into [Original Player]'s Hand (upgraded: upgrade it). Exhaust. Ethereal.
/// Given by Random Portal; the Original Player is the one who played it.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class SlalReturnPortal() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
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
        if (receiver is not { Creature.IsAlive: true }) return;

        var chosen = (await CardSelectCmd.FromHand(
            choiceContext,
            card.Owner,
            new CardSelectorPrefs(card.SelectionScreenPrompt, 1),
            null,
            card
        )).FirstOrDefault();
        if (chosen is null) return;

        await SlalMechanicsHelper.GiveCardToPlayer(chosen, receiver, PileType.Hand, IsUpgraded);
    }
}
