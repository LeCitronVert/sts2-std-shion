using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(SharedRelicPool))]
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