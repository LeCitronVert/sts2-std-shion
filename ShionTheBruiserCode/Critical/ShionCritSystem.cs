using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Critical;

/// <summary>
/// Central state manager for Shion's Critical Damage mechanic.
/// All state is static because STS2's game logic is single-threaded.
/// Uses a stack to handle nested attacks (e.g., when a critical hit triggers
/// another attack via a relic or power).
/// </summary>
public static class ShionCritSystem
{
    /// <summary>
    /// State for a single attack, tracking its crit rolls and current hit status.
    /// </summary>
    private class AttackState
    {
        public CardModel? Card;
        public bool CritAttack;
        public bool CurrentHitIsCrit;
        public bool HitCritRolled;
    }

    /// <summary>
    /// Stack of attack states. The top of the stack is the currently active attack.
    /// When a nested attack occurs (e.g., Abby attacking during a crit callback),
    /// it pushes a new state onto the stack.
    /// </summary>
    private static readonly Stack<AttackState> _attackStack = new();

    /// <summary>Whether the hit currently being resolved is a critical hit.</summary>
    public static bool CurrentHitIsCrit => _attackStack.Count > 0 && _attackStack.Peek().CurrentHitIsCrit;

    /// <summary>
    /// Called once at the beginning of each attack (<c>BeforeAttack</c> hook).
    /// Performs the whole-attack crit roll and resets per-hit state.
    /// Pushes a new attack state onto the stack to handle nested attacks.
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
        
        // Use proper game RNG for determinism (replays, seeding, etc.)
        float roll = card?.Owner.RunState.Rng.Niche.NextFloat() ?? 0f;
        
        var state = new AttackState
        {
            Card = card,
            CritAttack = (decimal)roll < attackRate,
            CurrentHitIsCrit = false,
            HitCritRolled = false
        };
        
        _attackStack.Push(state);
        
        MainFile.Logger.Info($"[ShionCritSystem] StartAttack (depth={_attackStack.Count}) — card={card?.GetType().Name ?? "null"}, attackRate={attackRate:P0}, roll={roll:F3}, critAttack={state.CritAttack}");
    }

    /// <summary>
    /// Performs a per-hit crit roll if one has not already been performed for this hit.
    /// Idempotent: subsequent calls within the same hit are no-ops (guarded by
    /// <see cref="AttackState.HitCritRolled"/>).
    /// Called inside <c>ModifyDamageMultiplicative</c> so the roll happens at the
    /// moment the damage is calculated, which also covers neutral cards (which do
    /// not inject a <c>BeforeDamage</c> callback).
    /// </summary>
    public static void EnsureHitRolled(ICombatState cs, CardModel? card)
    {
        if (_attackStack.Count == 0)
        {
            MainFile.Logger.Info($"[ShionCritSystem] EnsureHitRolled SKIPPED (no active attack) — card={card?.GetType().Name ?? "null"}");
            return;
        }

        var state = _attackStack.Peek();
        
        // STS2 recalculates card-preview damage for every card in hand immediately after
        // an attack starts. Each preview fires ModifyDamageMultiplicative with that card
        // as the source. Ignore any call whose source doesn't match the attacking card.
        if (card != state.Card)
        {
            MainFile.Logger.Info($"[ShionCritSystem] EnsureHitRolled SKIPPED (preview) — card={card?.GetType().Name ?? "null"}, expected={state.Card?.GetType().Name ?? "null"}");
            return;
        }

        if (state.HitCritRolled) return;
        
        decimal hitRate = card != null
            ? ShionHooksHelper.CalculateCritHitRate(cs, card)
            : 0m;
        
        // Use proper game RNG for determinism (replays, seeding, etc.)
        float roll = card?.Owner.RunState.Rng.Niche.NextFloat() ?? 0f;
        
        state.CurrentHitIsCrit = state.CritAttack || (decimal)roll < hitRate;
        state.HitCritRolled = true;
        
        MainFile.Logger.Info($"[ShionCritSystem] EnsureHitRolled (depth={_attackStack.Count}) — card={card?.GetType().Name ?? "null"}, hitRate={hitRate:P0}, roll={roll:F3}, critAttack={state.CritAttack}, currentHitIsCrit={state.CurrentHitIsCrit}");
    }

    /// <summary>
    /// Signals that the current individual hit has been fully resolved.
    /// Resets the per-hit roll guard so the next hit gets an independent roll.
    /// Called in <c>AfterDamageGiven</c> by the tracker power.
    /// Does NOT reset <see cref="AttackState.CurrentHitIsCrit"/> — that value remains valid
    /// until the next roll (needed by <c>AfterDamageGiven</c> for subsequent AOE
    /// targets that were damaged in the same <c>CreatureCmd.Damage</c> call).
    /// </summary>
    public static void EndHit()
    {
        if (_attackStack.Count > 0)
        {
            _attackStack.Peek().HitCritRolled = false;
            MainFile.Logger.Info($"[ShionCritSystem] EndHit (depth={_attackStack.Count})");
        }
    }

    /// <summary>
    /// Fully resets crit state after an attack has finished.
    /// Called in <c>AfterAttack</c> by the tracker power.
    /// Pops the current attack state from the stack, restoring the previous
    /// attack's state if this was a nested attack.
    /// </summary>
    public static void EndAttack()
    {
        if (_attackStack.Count > 0)
        {
            _attackStack.Pop();
            MainFile.Logger.Info($"[ShionCritSystem] EndAttack (remaining depth={_attackStack.Count})");
        }
        else
        {
            MainFile.Logger.Info($"[ShionCritSystem] EndAttack called but stack is empty!");
        }
    }
}
