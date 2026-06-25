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
        MainFile.Logger.Info($"[ShionHooksHelper] CalculateCritHitRate — card={card?.GetType().Name ?? "null"}");

        decimal rate = 0m;
        foreach (var l in combatState.IterateHookListeners().Where(l => l != card).OfType<ICritHitRateContributor>())
        {
            decimal bonus = l.GetCritHitRateBonus(card);
            MainFile.Logger.Info($"[ShionHooksHelper]   listener contributor {l.GetType().Name} → hitRateBonus={bonus:P0}");
            rate += bonus;
        }

        if (card is ICritHitRateContributor cardHitContributor)
        {
            decimal bonus = cardHitContributor.GetCritHitRateBonus(card);
            MainFile.Logger.Info($"[ShionHooksHelper]   direct-card contributor {card.GetType().Name} → hitRateBonus={bonus:P0}");
            rate += bonus;
        }
        else
        {
            MainFile.Logger.Info($"[ShionHooksHelper]   card {card?.GetType().Name ?? "null"} does NOT implement ICritHitRateContributor");
        }

        decimal minRate = 0m;
        foreach (var l in combatState.IterateHookListeners().Where(l => l != card).OfType<IMinCritHitRateContributor>())
        {
            decimal floor = l.GetMinCritHitRate(card);
            MainFile.Logger.Info($"[ShionHooksHelper]   listener minHit {l.GetType().Name} → floor={floor:P0}");
            minRate = Math.Max(minRate, floor);
        }

        if (card is IMinCritHitRateContributor cardMinHitContributor)
        {
            decimal floor = cardMinHitContributor.GetMinCritHitRate(card);
            MainFile.Logger.Info($"[ShionHooksHelper]   direct-card minHit {card.GetType().Name} → floor={floor:P0}");
            minRate = Math.Max(minRate, floor);
        }

        decimal result = Math.Max(rate, minRate);
        MainFile.Logger.Info($"[ShionHooksHelper] CalculateCritHitRate result: rate={rate:P0}, minRate={minRate:P0}, final={result:P0}");
        return result;
    }

    /// <summary>
    /// Calculates the effective whole-attack critical chance for the given card.
    /// If this rolls true, every hit of the attack will be critical regardless of the
    /// per-hit roll. Sums all <see cref="ICritAttackRateContributor"/> bonuses, then
    /// clamps to the floor set by <see cref="IMinCritAttackRateContributor"/> contributors.
    /// </summary>
    public static decimal CalculateCritAttackRate(ICombatState combatState, CardModel? card)
    {
        MainFile.Logger.Info($"[ShionHooksHelper] CalculateCritAttackRate — card={card?.GetType().Name ?? "null"}");

        decimal rate = 0m;
        foreach (var l in combatState.IterateHookListeners().Where(l => l != card).OfType<ICritAttackRateContributor>())
        {
            decimal bonus = l.GetCritAttackRateBonus(card);
            MainFile.Logger.Info($"[ShionHooksHelper]   listener contributor {l.GetType().Name} → attackRateBonus={bonus:P0}");
            rate += bonus;
        }

        if (card is ICritAttackRateContributor cardAttackContributor)
        {
            decimal bonus = cardAttackContributor.GetCritAttackRateBonus(card);
            MainFile.Logger.Info($"[ShionHooksHelper]   direct-card contributor {card.GetType().Name} → attackRateBonus={bonus:P0}");
            rate += bonus;
        }
        else
        {
            MainFile.Logger.Info($"[ShionHooksHelper]   card {card?.GetType().Name ?? "null"} does NOT implement ICritAttackRateContributor");
        }

        decimal minRate = 0m;
        foreach (var l in combatState.IterateHookListeners().Where(l => l != card).OfType<IMinCritAttackRateContributor>())
        {
            decimal floor = l.GetMinCritAttackRate(card);
            MainFile.Logger.Info($"[ShionHooksHelper]   listener minAttack {l.GetType().Name} → floor={floor:P0}");
            minRate = Math.Max(minRate, floor);
        }

        if (card is IMinCritAttackRateContributor cardMinAttackContributor)
        {
            decimal floor = cardMinAttackContributor.GetMinCritAttackRate(card);
            MainFile.Logger.Info($"[ShionHooksHelper]   direct-card minAttack {card.GetType().Name} → floor={floor:P0}");
            minRate = Math.Max(minRate, floor);
        }

        decimal result = Math.Max(rate, minRate);
        MainFile.Logger.Info($"[ShionHooksHelper] CalculateCritAttackRate result: rate={rate:P0}, minRate={minRate:P0}, final={result:P0}");
        return result;
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