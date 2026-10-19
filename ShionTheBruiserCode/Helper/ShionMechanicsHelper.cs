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
    
    // Galvanised X : active while you have at least X Vigor.
    // Spirit Break disables Galvanised effects, Fell Dragon always applies them.
    // bonusVigor lets cards check against Vigor they're about to gain (for glowing).
    public static bool IsGalvanised(Creature creature, int threshold, int bonusVigor = 0)
    {
        if (creature.HasPower<SpiritBreak>()) return false;
        if (creature.HasPower<FellDragon>()) return true;
        
        return creature.GetPowerAmount<VigorPower>() + bonusVigor >= threshold;
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

    public static async Task EnsureCreatureHasCritTracker(Creature creature)
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
    
    public static async Task EnsureCreatureHasBerserkIndicator(Creature creature)
    {
        try
        {
            if (creature.HasPower<BerserkIndicator>()) return;

            await PowerCmd.Apply<BerserkIndicator>(
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
            
            Log.Error("Berserk crashed : " + e.Message);
        }
    }

    public static async Task ApplyCriticalDraw(CardModel criticalDrawCard, PlayerChoiceContext choiceContext, CardModel cardDrawn)
    {
        if (criticalDrawCard != cardDrawn) return;

        var owner = criticalDrawCard.Owner.Creature;
        var critDrawAmount = criticalDrawCard.DynamicVars["CriticalDraw"].BaseValue;
        
        if (critDrawAmount <= 0) return;
        
        await EnsureCreatureHasCritTracker(owner);
        
        await PowerCmd.Apply<CriticalDraw>(
            choiceContext,
            owner,
            critDrawAmount,
            owner,
            criticalDrawCard
        );
    }
    
    public const decimal BaseBerserkThreshold = 0.5m;
    public const decimal BasePatchUpThreshold = 0.5m;
    
    // Ratio of Max HP at or below which the creature is Berserk (50% by default)
    public static decimal GetBerserkThreshold(Creature creature)
    {
        var threshold = BaseBerserkThreshold + (creature.GetPower<PleasureInPain>()?.Amount ?? 0) / 100m;
        return Math.Clamp(threshold, 0m, 1m);
    }
    
    // Ratio of Max HP Patch Up can heal up to (50% by default)
    public static decimal GetPatchUpThreshold(Creature creature)
    {
        var threshold = BasePatchUpThreshold + (creature.GetPower<PleasureInPain>()?.Amount ?? 0) / 100m;
        if (creature.HasPower<FellDragon>())
        {
            threshold -= FellDragon.PatchUpReduction / 100m;
        }
        return Math.Clamp(threshold, 0m, 1m);
    }
    
    // Checks whether creature is at or below its Berserk threshold.
    // Endure disables Berserk effects, Fell Dragon always applies them.
    public static bool IsBerserk(Creature creature)
    {
        if (creature.HasPower<Endure>()) return false;
        if (creature.HasPower<FellDragon>()) return true;
        
        return creature.CurrentHp <= creature.MaxHp * GetBerserkThreshold(creature);
    }
    
    // Patch Up : Heals by X amount until you're at the Patch Up threshold (50%HP by default)
    public static async Task PatchUpCreature(int amount, Creature creature)
    {
        var patchUpLimit = (int) (creature.MaxHp * GetPatchUpThreshold(creature));
        var amountToHeal = Math.Min(amount, patchUpLimit - creature.CurrentHp);
        
        if (amountToHeal <= 0) return;
        
        await CreatureCmd.Heal(
            creature,
            amountToHeal
        );
    }
}