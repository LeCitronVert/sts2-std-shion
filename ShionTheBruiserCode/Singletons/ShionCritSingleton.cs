// Critical/ShionCritSingleton.cs

using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Singletons;

/// <summary>
/// Global combat-hook singleton driving Shion's Critical Damage mechanic.
/// Replaces the old static <c>ShionCritSystem</c> class: BaseLib's
/// <see cref="CustomSingletonModel"/> with <c>HookType.Combat</c> receives
/// combat hooks for every creature automatically, so this class IS the
/// hook listener now instead of being invoked by a per-creature power.
///
/// Because it fires globally, every entry point gates on whether the
/// acting creature currently carries <see cref="ShionCritTrackerPower"/>
/// (still granted per-creature by CritContributorPower/CritContributorRelic).
/// That reproduces the old per-owner scoping — creatures with no crit
/// contributor never push attack state or consume an RNG roll.
/// </summary>
public class ShionCritSingleton : CustomSingletonModel
{
    // Self-registers on construction so other files can reach the one
    // instance the game creates at ModelDb load time. If BaseLib exposes
    // its own accessor (e.g. ModelDb.Get<T>()) prefer that instead —
    // check the decompiled source / BaseLib repo to confirm before relying
    // on this fallback long-term.
    public static ShionCritSingleton? Instance { get; private set; }

    public ShionCritSingleton() : base(HookType.Combat)
    {
        Instance = this;
    }

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
    /// Single shared stack, same as the old static class: attacks resolve
    /// sequentially (single-threaded), so this only needs to handle
    /// self-nesting (a crit triggering another attack on the same
    /// creature), not concurrent attacks from different creatures.
    /// </summary>
    private readonly Stack<AttackState> _attackStack = new();

    /// <summary>Whether the hit currently being resolved is a critical hit.</summary>
    public bool CurrentHitIsCrit => _attackStack.Count > 0 && _attackStack.Peek().CurrentHitIsCrit;

    /// <summary>Clears all stacked attack state at the start of every combat, globally, once.</summary>
    public override Task BeforeCombatStart()
    {
        int cleared = _attackStack.Count;
        _attackStack.Clear();
        MainFile.Logger.Info($"[ShionCritSingleton] BeforeCombatStart — cleared {cleared} stale attack state(s)");
        return Task.CompletedTask;
    }

    /// <summary>Belt-and-suspenders clear in case anything is left in-flight when combat ends.</summary>
    public override Task AfterCombatEnd(CombatRoom room)
    {
        int cleared = _attackStack.Count;
        _attackStack.Clear();
        MainFile.Logger.Info($"[ShionCritSingleton] AfterCombatEnd — cleared {cleared} stale attack state(s)");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Fires once before every attack in combat. Performs the whole-attack
    /// crit roll and pushes new attack state — but only for creatures
    /// carrying the tracker power.
    /// </summary>
    public override Task BeforeAttack(AttackCommand command)
    {
        if (command.Attacker == null || !command.Attacker.HasPower<ShionCritTrackerPower>())
            return Task.CompletedTask;

        var card = command.ModelSource as CardModel;
        decimal attackRate = card != null
            ? ShionHooksHelper.CalculateCritAttackRate(command.Attacker.CombatState!, card)
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

        MainFile.Logger.Info($"[ShionCritSingleton] StartAttack (depth={_attackStack.Count}) — attacker={command.Attacker.GetType().Name}, card={card?.GetType().Name ?? "null"}, attackRate={attackRate:P0}, roll={roll:F3}, critAttack={state.CritAttack}");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Applies the 1.5× critical multiplier per damage-calculation call.
    /// Must stay side-effect free: card previews call this too, at times that differ per client,
    /// so rolling here desyncs multiplayer. The per-hit roll happens in <see cref="RollPendingHit"/>.
    /// </summary>
    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == null || !dealer.HasPower<ShionCritTrackerPower>() || !props.IsPoweredAttack())
            return 1m;

        return CurrentHitIsCrit ? 1.5m : 1m;
    }

    /// <summary>
    /// Performs the per-hit crit roll, if one is pending. Only called from
    /// <see cref="Patches.CritHitRollPatch"/> right before real damage is dealt, never from previews.
    /// </summary>
    public void RollPendingHit(Creature? dealer, ValueProp props, CardModel? cardSource)
    {
        if (dealer == null || !dealer.HasPower<ShionCritTrackerPower>() || !props.IsPoweredAttack())
            return;

        EnsureHitRolled(dealer.CombatState!, cardSource);
    }

    private void EnsureHitRolled(ICombatState cs, CardModel? card)
    {
        if (_attackStack.Count == 0)
        {
            MainFile.Logger.Info($"[ShionCritSingleton] EnsureHitRolled SKIPPED (no active attack) — card={card?.GetType().Name ?? "null"}");
            return;
        }

        var state = _attackStack.Peek();

        // Damage from another source (e.g. a crit listener's follow-up) doesn't get its own roll.
        if (card != state.Card)
        {
            MainFile.Logger.Info($"[ShionCritSingleton] EnsureHitRolled SKIPPED (other source) — card={card?.GetType().Name ?? "null"}, expected={state.Card?.GetType().Name ?? "null"}");
            return;
        }

        if (state.HitCritRolled) return;

        decimal hitRate = card != null
            ? ShionHooksHelper.CalculateCritHitRate(cs, card)
            : 0m;

        float roll = card?.Owner.RunState.Rng.Niche.NextFloat() ?? 0f;

        state.CurrentHitIsCrit = state.CritAttack || (decimal)roll < hitRate;
        state.HitCritRolled = true;

        MainFile.Logger.Info($"[ShionCritSingleton] EnsureHitRolled (depth={_attackStack.Count}) — hitRate={hitRate:P0}, roll={roll:F3}, critAttack={state.CritAttack}, currentHitIsCrit={state.CurrentHitIsCrit}");
    }

    /// <summary>
    /// Plays critical-hit SFX/VFX and dispatches OnCriticalHit/OnCriticalFail
    /// listeners, then resets the per-hit roll guard.
    /// </summary>
    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == null || !dealer.HasPower<ShionCritTrackerPower>() || !props.IsPoweredAttack()) return;
        bool isCrit = CurrentHitIsCrit;

        if (isCrit && result.UnblockedDamage > 0)
        {
            // TODO: replace with real FMOD event / VFX scene paths once assets are ready.
            TalkCmd.Play(new LocString("combat_messages", "SHIONTHEBRUISER-CRIT"), dealer, VfxColor.Purple, VfxDuration.VeryShort);
        }

        if (isCrit)
        {
            foreach (var listener in dealer.CombatState!.IterateHookListeners().OfType<IOnCriticalHitListener>())
            {
                await listener.OnCriticalHit(dealer, target, result, props, cardSource, choiceContext);
            }
        }
        else
        {
            foreach (var listener in dealer.CombatState!.IterateHookListeners().OfType<IOnCriticalFailListener>())
            {
                await listener.OnCriticalFail(dealer, target, result, props, cardSource, choiceContext);
            }
        }

        if (_attackStack.Count > 0)
        {
            _attackStack.Peek().HitCritRolled = false;
            MainFile.Logger.Info($"[ShionCritSingleton] EndHit (depth={_attackStack.Count})");
        }
    }

    /// <summary>Pops the attack state once the whole attack sequence finishes.</summary>
    public override Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != null && command.Attacker.HasPower<ShionCritTrackerPower>() && _attackStack.Count > 0)
        {
            _attackStack.Pop();
            MainFile.Logger.Info($"[ShionCritSingleton] EndAttack (remaining depth={_attackStack.Count})");
        }

        return Task.CompletedTask;
    }
}