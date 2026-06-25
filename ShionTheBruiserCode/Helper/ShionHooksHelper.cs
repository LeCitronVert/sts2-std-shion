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
        decimal rate = combatState.IterateHookListeners()
            .OfType<ICritHitRateContributor>()
            .Sum(c => c.GetCritHitRateBonus(card));

        decimal minRate = combatState.IterateHookListeners()
            .OfType<IMinCritHitRateContributor>()
            .Select(c => c.GetMinCritHitRate(card))
            .DefaultIfEmpty(0m)
            .Max();

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
        decimal rate = combatState.IterateHookListeners()
            .OfType<ICritAttackRateContributor>()
            .Sum(c => c.GetCritAttackRateBonus(card));

        decimal minRate = combatState.IterateHookListeners()
            .OfType<IMinCritAttackRateContributor>()
            .Select(c => c.GetMinCritAttackRate(card))
            .DefaultIfEmpty(0m)
            .Max();

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