using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Critical;

/// <summary>
/// Central state manager for Shion's Critical Damage mechanic.
/// All state is static because STS2's game logic is single-threaded.
/// The <see cref="Relics.ShionCritTrackerRelic"/> drives this class via combat hooks.
/// </summary>
public static class ShionCritSystem
{
    /// <summary>
    /// True when the whole-attack critical roll succeeded for the current attack.
    /// If true, every hit of the attack is a critical hit regardless of the per-hit roll.
    /// </summary>
    private static bool _critAttack;

    /// <summary>
    /// True when the current individual hit is a critical hit.
    /// Set inside <see cref="EnsureHitRolled"/> and read by the tracker relic's
    /// <c>ModifyDamageMultiplicative</c> and <c>AfterDamageGiven</c> overrides.
    /// </summary>
    private static bool _currentHitIsCrit;

    /// <summary>
    /// Guard flag: true once a per-hit crit roll has been performed for the current hit.
    /// Prevents re-rolling when multiple targets are hit by the same attack call,
    /// or when <see cref="EnsureHitRolled"/> is called more than once.
    /// Reset by <see cref="EndHit"/> between hits.
    /// </summary>
    private static bool _hitCritRolled;

    /// <summary>Whether the hit currently being resolved is a critical hit.</summary>
    public static bool CurrentHitIsCrit => _currentHitIsCrit;

    /// <summary>
    /// Called once at the beginning of each attack (<c>BeforeAttack</c> hook).
    /// Performs the whole-attack crit roll and resets per-hit state.
    /// </summary>
    /// <param name="cs">Current combat state used to iterate hook listeners.</param>
    /// <param name="card">
    /// The card that started the attack, or <c>null</c> for card-less attacks.
    /// Passed to contributor interfaces so they can filter by card type if needed.
    /// </param>
    public static void StartAttack(ICombatState cs, CardModel? card)
    {
        decimal attackRate = card != null
            ? ShionHooksHelper.CalculateCritAttackRate(cs, card)
            : 0m;
        _critAttack = (decimal)GD.Randf() < attackRate;
        _currentHitIsCrit = false;
        _hitCritRolled = false;
    }

    /// <summary>
    /// Performs a per-hit crit roll if one has not already been performed for this hit.
    /// Idempotent: subsequent calls within the same hit are no-ops (guarded by
    /// <see cref="_hitCritRolled"/>).
    /// Called inside <c>ModifyDamageMultiplicative</c> so the roll happens at the
    /// moment the damage is calculated, which also covers neutral cards (which do
    /// not inject a <c>BeforeDamage</c> callback).
    /// </summary>
    public static void EnsureHitRolled(ICombatState cs, CardModel? card)
    {
        if (_hitCritRolled) return;
        decimal hitRate = card != null
            ? ShionHooksHelper.CalculateCritHitRate(cs, card)
            : 0m;
        _currentHitIsCrit = _critAttack || (decimal)GD.Randf() < hitRate;
        _hitCritRolled = true;
    }

    /// <summary>
    /// Signals that the current individual hit has been fully resolved.
    /// Resets the per-hit roll guard so the next hit gets an independent roll.
    /// Called in <c>AfterDamageGiven</c> by the tracker relic.
    /// Does NOT reset <see cref="_currentHitIsCrit"/> — that value remains valid
    /// until the next roll (needed by <c>AfterDamageGiven</c> for subsequent AOE
    /// targets that were damaged in the same <c>CreatureCmd.Damage</c> call).
    /// </summary>
    public static void EndHit() => _hitCritRolled = false;

    /// <summary>
    /// Fully resets all crit state after an attack has finished.
    /// Called in <c>AfterAttack</c> by the tracker relic.
    /// </summary>
    public static void EndAttack()
    {
        _critAttack = false;
        _currentHitIsCrit = false;
        _hitCritRolled = false;
    }
}

