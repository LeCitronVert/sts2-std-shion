using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Another player chooses 1 out of [number of players] Form Powers, one from each player's character.
/// It is free to play this turn. Exhaust.
/// </summary>
public class SlalPalArmor() : SlalTheEndlessCard(3,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;
        ArgumentNullException.ThrowIfNull(play.Target?.Player);
        var receiver = play.Target.Player;

        // One random Form from each player's character pool (two players with the same character offer two different ones)
        List<CardModel> options = [];
        foreach (var player in card.CombatState.Players)
        {
            var forms = player.Character.CardPool
                .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
                .Where(c => SlalMechanicsHelper.IsFormPower(c) && c.CanBeGeneratedInCombat && options.All(o => o.Id != c.Id))
                .ToList();
            var form = CardFactory.GetDistinctForCombat(receiver, forms, 1, receiver.RunState.Rng.CombatCardGeneration).FirstOrDefault();
            if (form != null) options.Add(form);
        }
        if (options.Count == 0) return;

        // The other player makes the choice. The choose-a-card screen only fits 3 cards.
        var chosen = options.Count <= 3
            ? await CardSelectCmd.FromChooseACardScreen(choiceContext, options, receiver)
            : (await CardSelectCmd.FromSimpleGrid(choiceContext, options, receiver, new CardSelectorPrefs(card.SelectionScreenPrompt, 1))).FirstOrDefault();
        if (chosen is null) return;

        await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, receiver);
        chosen.SetToFreeThisTurn();
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
