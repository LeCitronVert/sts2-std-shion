using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Critical;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

public static class ShionHooksHelper
{
    /// <summary>
    /// Calculates the effective per-hit critical chance for the given card.
    /// Sums all <see cref="ICritHitRateContributor"/> bonuses from current combat hook
    /// listeners, then clamps the result to be at least the highest floor set by any
    /// <see cref="IMinCritHitRateContributor"/>.
    /// </summary>
    public static decimal CalculateCritHitRate(ICombatState combatState, CardModel? card)
    {
        // Exclude the source card from the listener query — it is checked directly below.
        // Cards often leave the hook-listener set before ModifyDamageMultiplicative fires
        // (they're no longer in the hand), so we can't rely on IterateHookListeners() alone.
        // The explicit exclusion also prevents double-counting on the rare occasion the card
        // IS still in the listener set.
        decimal rate = combatState.IterateHookListeners()
            .Where(l => l != card)
            .OfType<ICritHitRateContributor>()
            .Sum(c => c.GetCritHitRateBonus(card));

        if (card is ICritHitRateContributor cardHitContributor)
            rate += cardHitContributor.GetCritHitRateBonus(card);

        decimal minRate = combatState.IterateHookListeners()
            .Where(l => l != card)
            .OfType<IMinCritHitRateContributor>()
            .Select(c => c.GetMinCritHitRate(card))
            .DefaultIfEmpty(0m)
            .Max();

        if (card is IMinCritHitRateContributor cardMinHitContributor)
            minRate = Math.Max(minRate, cardMinHitContributor.GetMinCritHitRate(card));

        return Math.Max(rate, minRate);
    }

    /// <summary>
    /// Calculates the effective whole-attack critical chance for the given card.
    /// If this rolls true, every hit of the attack will be critical regardless of the
    /// per-hit roll. Sums all <see cref="ICritAttackRateContributor"/> bonuses, then
    /// clamps to the floor set by <see cref="IMinCritAttackRateContributor"/> contributors.
    /// </summary>
    public static decimal CalculateCritAttackRate(ICombatState combatState, CardModel? card)
    {
        // Same direct-card logic as CalculateCritHitRate — see comments there.
        decimal rate = combatState.IterateHookListeners()
            .Where(l => l != card)
            .OfType<ICritAttackRateContributor>()
            .Sum(c => c.GetCritAttackRateBonus(card));

        if (card is ICritAttackRateContributor cardAttackContributor)
            rate += cardAttackContributor.GetCritAttackRateBonus(card);

        decimal minRate = combatState.IterateHookListeners()
            .Where(l => l != card)
            .OfType<IMinCritAttackRateContributor>()
            .Select(c => c.GetMinCritAttackRate(card))
            .DefaultIfEmpty(0m)
            .Max();

        if (card is IMinCritAttackRateContributor cardMinAttackContributor)
            minRate = Math.Max(minRate, cardMinAttackContributor.GetMinCritAttackRate(card));

        return Math.Max(rate, minRate);
    }

    /// <summary>
    /// Returns only the minimum floor for per-hit crit chance, without adding any
    /// contributor bonuses. Useful when you need to inspect the floor in isolation.
    /// </summary>
    public static decimal CalculateMinCritHitRate(ICombatState combatState, CardModel card)
    {
        return combatState.IterateHookListeners()
            .OfType<IMinCritHitRateContributor>()
            .Select(c => c.GetMinCritHitRate(card))
            .DefaultIfEmpty(0m)
            .Max();
    }

    /// <summary>
    /// Returns only the minimum floor for whole-attack crit chance, without adding any
    /// contributor bonuses. Useful when you need to inspect the floor in isolation.
    /// </summary>
    public static decimal CalculateMinCritAttackRate(ICombatState combatState, CardModel card)
    {
        return combatState.IterateHookListeners()
            .OfType<IMinCritAttackRateContributor>()
            .Select(c => c.GetMinCritAttackRate(card))
            .DefaultIfEmpty(0m)
            .Max();
    }
}