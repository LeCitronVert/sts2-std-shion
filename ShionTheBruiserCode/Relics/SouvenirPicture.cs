using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class SouvenirPicture() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Common;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || Owner.PlayerCombatState?.TurnNumber != 1)
        {
            return;
        }
        
        var drawPile = PileType.Draw.GetPile(player);
        int handSize = 5;
        
        var nextCards = drawPile.Cards.Take(handSize).ToList();
        
        if (nextCards.Count == 0 || player.Creature.CombatState == null)
            return;
        
        // Show preview to player using FromSimpleGrid with 0 selection required
        await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            nextCards,
            player,
            new CardSelectorPrefs(
                new LocString("relics", "SHIONTHEBRUISER-SOUVENIR_PICTURE.preview"), 
                0, 
                0
                )
        );
        
        // Now offer the Yes/No choice for shuffling
        CardModel? choice = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            new List<CardModel> {
                player.Creature.CombatState.CreateCard<Yes>(player),
                player.Creature.CombatState.CreateCard<No>(player)
            },
            player,
            canSkip: false
        );
        
        if (choice is Yes)
        {
            await CardPileCmd.Shuffle(choiceContext, player);
        }
    }
}