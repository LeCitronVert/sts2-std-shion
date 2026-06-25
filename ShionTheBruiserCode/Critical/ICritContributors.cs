using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Critical;

/// <summary>
/// Implement this on a card, power, or relic to add a bonus to the per-hit critical chance
/// for Shion's attacks. Bonuses from all contributors are summed together.
/// </summary>
public interface ICritHitRateContributor
{
    /// <summary>
    /// Returns the additional per-hit critical hit chance this contributor adds.
    /// Use values in the [0, 1] range (e.g. 0.1m = 10%).
    /// </summary>
    /// <param name="sourceCard">The attack card being played, or null if not from a card.</param>
    decimal GetCritHitRateBonus(CardModel? sourceCard);
}

/// <summary>
/// Implement this on a card, power, or relic to add a bonus to the whole-attack critical chance
/// for Shion's attacks. If a critical attack is rolled, ALL hits of that attack are critical.
/// Bonuses from all contributors are summed together.
/// </summary>
public interface ICritAttackRateContributor
{
    /// <summary>
    /// Returns the additional whole-attack critical chance this contributor adds.
    /// Use values in the [0, 1] range (e.g. 0.1m = 10%).
    /// </summary>
    /// <param name="sourceCard">The attack card being played, or null if not from a card.</param>
    decimal GetCritAttackRateBonus(CardModel? sourceCard);
}

/// <summary>
/// Implement this on a card, power, or relic to impose a minimum floor on the per-hit
/// critical hit chance. The highest minimum across all contributors wins.
/// </summary>
public interface IMinCritHitRateContributor
{
    /// <summary>
    /// Returns the minimum per-hit critical hit chance this contributor guarantees.
    /// The effective floor is the maximum value across all <see cref="IMinCritHitRateContributor"/> instances.
    /// Use values in the [0, 1] range (e.g. 0.05m = 5% guaranteed).
    /// </summary>
    /// <param name="sourceCard">The attack card being played, or null if not from a card.</param>
    decimal GetMinCritHitRate(CardModel? sourceCard);
}

/// <summary>
/// Implement this on a card, power, or relic to impose a minimum floor on the whole-attack
/// critical chance. The highest minimum across all contributors wins.
/// </summary>
public interface IMinCritAttackRateContributor
{
    /// <summary>
    /// Returns the minimum whole-attack critical chance this contributor guarantees.
    /// The effective floor is the maximum value across all <see cref="IMinCritAttackRateContributor"/> instances.
    /// Use values in the [0, 1] range (e.g. 0.05m = 5% guaranteed).
    /// </summary>
    /// <param name="sourceCard">The attack card being played, or null if not from a card.</param>
    decimal GetMinCritAttackRate(CardModel? sourceCard);
}

