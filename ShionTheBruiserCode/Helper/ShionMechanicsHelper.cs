using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

public static class ShionMechanicsHelper
{
    public static int GetDragonUniqueBuffCount(Creature creature)
    {
        // Get count of unique Buffs.
        return creature.Powers.Where(p => p.Type == PowerType.Buff).DistinctBy(p => p.GetType()).Count();
    }
    
    public static bool HasVigor(Creature creature) 
    {
        return creature.HasPower<VigorPower>();
    }
    
    public static int GetStrengthAmount(Creature creature)
    {
        return creature.GetPower<StrengthPower>()?.Amount ?? 0;
    }
    
    public static bool IsCardNonNativeToShion(CardModel card)
    {
        return card.Pool.Id != ModelDb.CardPool<ShionTheBruiserCardPool>().Id;
    }
    
    public static bool IsCardNativeToCharacter(CardModel card, CharacterModel character)
    {
        return card.Pool.Id == character.CardPool.Id;
    }
    
    public static IEnumerable<CardModel> GetAllCombatCards (Player player)
    {
        var handCards = PileType.Hand.GetPile(player).Cards;
        var drawCards = PileType.Draw.GetPile(player).Cards;
        var discardCards = PileType.Discard.GetPile(player).Cards;
        
        return handCards.Concat(drawCards).Concat(discardCards);
    }
    
    public static IEnumerable<CardModel> GetRandomCardOptionsForPool (IEnumerable<CardPoolModel> pools, Player player)
    {
        List<CardModel> randomOptions = [];

        foreach (var pool in pools)
        {
            randomOptions = randomOptions.Concat(pool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)).ToList();
        }

        return randomOptions;
    }

    public static async void EnsureCreatureHasCritTracker(Creature creature)
    {
        try
        {
            if (creature.HasPower<ShionCritTrackerPower>()) return;

            await PowerCmd.Apply<ShionCritTrackerPower>(
                new ThrowingPlayerChoiceContext(),
                creature,
                1,
                creature,
                null,
                true
            );
        }
        catch (Exception e)
        {
            
            Log.Error("Crit crashed : " + e.Message);
        }
    }
}