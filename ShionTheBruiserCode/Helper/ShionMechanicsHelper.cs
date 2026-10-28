using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
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

/// <summary>
/// Implemented by models that want to react to Patch Up.
/// </summary>
public interface IPatchUpListener
{
    /// <summary>Called after a creature Patches Up, with the HP it actually healed.</summary>
    Task AfterPatchUp(PlayerChoiceContext choiceContext, Creature creature, int amount);
}

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
    // Spirit Break and Defensive disable Galvanised effects, Fell Dragon always applies them.
    // bonusVigor lets cards check against Vigor they're about to gain (for glowing).
    public static bool IsGalvanised(Creature creature, int threshold, int bonusVigor = 0)
    {
        if (creature.HasPower<SpiritBreak>()) return false;
        if (creature.HasPower<DefensiveDebuff>()) return false;
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
    // Endure and Defensive disable Berserk effects, Fell Dragon always applies them.
    // Defensive cards pass ignoreDefensive for their own Berserk effects, so they still apply.
    public static bool IsBerserk(Creature creature, bool ignoreDefensive = false)
    {
        if (creature.HasPower<Endure>()) return false;
        if (!ignoreDefensive && creature.HasPower<DefensiveDebuff>()) return false;
        if (creature.HasPower<FellDragon>()) return true;
        
        return creature.CurrentHp <= creature.MaxHp * GetBerserkThreshold(creature);
    }
    
    // Defensive cards can't be played in a turn where a Berserk or Galvanised effect triggered.
    // Only call the Trigger* methods from real paths (OnPlay, After*/Before* hooks), never from previews,
    // glows or Modify* hooks.
    private sealed record SpecialEffectTrigger(WeakReference<ICombatState> Combat, int Turn);
    private static readonly ConditionalWeakTable<Creature, SpecialEffectTrigger> _lastSpecialEffectTrigger = new();

    // Berserk-only tracking (Souvenir Picture). Defensive cards whose own Berserk ignores Defensive mark only this one,
    // so they don't lock the other Defensive cards.
    private static readonly ConditionalWeakTable<Creature, SpecialEffectTrigger> _lastBerserkTrigger = new();

    public static void MarkSpecialEffectTriggered(Creature creature) => MarkTriggered(_lastSpecialEffectTrigger, creature);

    public static bool SpecialEffectTriggeredThisTurn(Creature creature) => TriggeredThisTurn(_lastSpecialEffectTrigger, creature);

    public static void MarkBerserkUsed(Creature creature) => MarkTriggered(_lastBerserkTrigger, creature);

    public static bool BerserkUsedThisTurn(Creature creature) => TriggeredThisTurn(_lastBerserkTrigger, creature);

    private static void MarkTriggered(ConditionalWeakTable<Creature, SpecialEffectTrigger> table, Creature creature)
    {
        if (creature.CombatState is not { } combatState) return;
        table.AddOrUpdate(creature, new SpecialEffectTrigger(new WeakReference<ICombatState>(combatState), GetTurnNumber(creature, combatState)));
    }

    private static bool TriggeredThisTurn(ConditionalWeakTable<Creature, SpecialEffectTrigger> table, Creature creature)
    {
        if (creature.CombatState is not { } combatState) return false;
        if (!table.TryGetValue(creature, out var trigger)) return false;

        return trigger.Combat.TryGetTarget(out var triggerCombat)
            && triggerCombat == combatState
            && trigger.Turn == GetTurnNumber(creature, combatState);
    }

    // The player's own turn count, so extra turns (which don't start a new round) count as new turns
    private static int GetTurnNumber(Creature creature, ICombatState combatState)
    {
        return creature.Player?.PlayerCombatState?.TurnNumber ?? combatState.RoundNumber;
    }

    // IsBerserk, and marks the Berserk effect as triggered if it applies
    public static bool TriggerBerserk(Creature creature)
    {
        if (!IsBerserk(creature)) return false;
        MarkSpecialEffectTriggered(creature);
        MarkBerserkUsed(creature);
        return true;
    }

    // IsGalvanised, and marks the Galvanised effect as triggered if it applies
    public static bool TriggerGalvanised(Creature creature, int threshold)
    {
        if (!IsGalvanised(creature, threshold)) return false;
        MarkSpecialEffectTriggered(creature);
        return true;
    }

    // Patch Up : Heals by X amount until you're at the Patch Up threshold (50%HP by default)
    /// <summary>
    /// HP a Patch Up of this amount would actually restore (capped by the Patch Up limit).
    /// </summary>
    public static int GetPatchUpHealAmount(int amount, Creature creature)
    {
        var patchUpLimit = (int) (creature.MaxHp * GetPatchUpThreshold(creature));
        return Math.Max(0, Math.Min(amount, patchUpLimit - creature.CurrentHp));
    }

    public static async Task PatchUpCreature(int amount, Creature creature)
    {
        var amountToHeal = GetPatchUpHealAmount(amount, creature);
        
        if (amountToHeal <= 0) return;
        
        await CreatureCmd.Heal(
            creature,
            amountToHeal
        );

        if (creature.CombatState is null) return;
        foreach (var listener in creature.CombatState.IterateHookListeners().OfType<IPatchUpListener>().ToList())
        {
            await listener.AfterPatchUp(new ThrowingPlayerChoiceContext(), creature, amountToHeal);
        }
    }
}