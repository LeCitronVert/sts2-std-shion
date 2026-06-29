using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Critical;

/// <summary>
/// Central state manager for Shion's Critical Damage mechanic.
/// All state is static because STS2's game logic is single-threaded.
/// </summary>
public static class ShionCritSystem
{
    /// <summary>
    /// True when the whole-attack critical roll succeeded for the current attack.
    /// If true, every hit of the attack is a critical hit regardless of the per-hit roll.
    /// </summary>
    private static bool _critAttack;
    private static bool _currentHitIsCrit;
    private static bool _hitCritRolled;

    /// <summary>
    /// The card whose attack is currently being resolved.
    /// Set by <see cref="StartAttack"/> and used by <see cref="EnsureHitRolled"/> to
    /// ignore STS2's card-preview recalculations, which call ModifyDamageMultiplicative
    /// for every card in hand (with that card as the source) immediately after the attack starts.
    /// Without this guard, those preview calls consume <see cref="_hitCritRolled"/> before
    /// the actual damage calculation for the attacking card runs.
    /// </summary>
    private static CardModel? _currentAttackCard;

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
        _currentAttackCard = card;
        decimal attackRate = card != null
            ? ShionHooksHelper.CalculateCritAttackRate(cs, card)
            : 0m;
        float roll = GD.Randf();
        _critAttack = (decimal)roll < attackRate;
        _currentHitIsCrit = false;
        _hitCritRolled = false;
        MainFile.Logger.Info($"[ShionCritSystem] StartAttack — card={card?.GetType().Name ?? "null"}, attackRate={attackRate:P0}, roll={roll:F3}, _critAttack={_critAttack}");
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
        // STS2 recalculates card-preview damage for every card in hand immediately after
        // an attack starts. Each preview fires ModifyDamageMultiplicative with that card
        // as the source. Ignore any call whose source doesn't match the attacking card —
        // otherwise the first preview (e.g. ShionStrike) steals _hitCritRolled before the
        // actual ShionCriticalStrike damage calculation runs.
        if (card != _currentAttackCard)
        {
            MainFile.Logger.Info($"[ShionCritSystem] EnsureHitRolled SKIPPED (preview) — card={card?.GetType().Name ?? "null"}, expected={_currentAttackCard?.GetType().Name ?? "null"}");
            return;
        }

        if (_hitCritRolled) return;
        decimal hitRate = card != null
            ? ShionHooksHelper.CalculateCritHitRate(cs, card)
            : 0m;
        float roll = GD.Randf();
        _currentHitIsCrit = _critAttack || (decimal)roll < hitRate;
        _hitCritRolled = true;
        MainFile.Logger.Info($"[ShionCritSystem] EnsureHitRolled — card={card?.GetType().Name ?? "null"}, hitRate={hitRate:P0}, roll={roll:F3}, _critAttack={_critAttack}, _currentHitIsCrit={_currentHitIsCrit}");
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
        _currentAttackCard = null;
        _critAttack = false;
        _currentHitIsCrit = false;
        _hitCritRolled = false;
    }
}
