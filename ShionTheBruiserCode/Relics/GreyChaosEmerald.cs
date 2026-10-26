using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// At the start of combat, choose a non-Eternal card in your deck and add a copy of it to your Hand.
/// At the end of combat, the chosen card is removed from your deck.
/// </summary>
public class GreyChaosEmerald() : ChaosEmeraldRelic
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Grey;

    private bool _triggeredThisCombat;
    private CardModel? _cardToRemove;

    public override Task BeforeCombatStart()
    {
        _triggeredThisCombat = false;
        _cardToRemove = null;
        return Task.CompletedTask;
    }

    // Runs on the first turn start (after the opening hand), since card choices need a PlayerChoiceContext.
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || _triggeredThisCombat) return;
        _triggeredThisCombat = true;

        var candidates = Owner.Deck.Cards.Where(c => !c.Keywords.Contains(CardKeyword.Eternal)).ToList();
        if (candidates.Count == 0) return;

        var prefs = new CardSelectorPrefs(new LocString("relics", "SHIONTHEBRUISER-GREY_CHAOS_EMERALD.selectionScreenPrompt"), 0, 1);
        var chosen = (await CardSelectCmd.FromSimpleGrid(choiceContext, candidates, Owner, prefs)).FirstOrDefault();
        var combatState = Owner.Creature.CombatState;
        if (chosen is null || combatState is null) return;

        Flash();
        _cardToRemove = chosen;
        CardCmd.PreviewCardPileAdd(
            await CardPileCmd.AddGeneratedCardToCombat(combatState.CloneCard(chosen), PileType.Hand, Owner)
        );
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var card = _cardToRemove;
        _cardToRemove = null;
        if (card is not null && Owner.Deck.Cards.Contains(card))
        {
            await CardPileCmd.RemoveFromDeck(card);
        }
    }
}
