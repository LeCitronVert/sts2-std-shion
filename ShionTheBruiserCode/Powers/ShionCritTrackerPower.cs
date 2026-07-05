using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Internal tracker power that drives Shion's Critical Damage system.
///
/// This power is applied automatically to any creature whose combat-active
/// models include at least one <see cref="Critical.ICritHitRateContributor"/>,
/// <see cref="Critical.ICritAttackRateContributor"/>, or the minimum-rate
/// equivalents. It is applied via
/// <see cref="Critical.CritContributorPower.BeforeCombatStart"/> or
/// <see cref="Critical.CritContributorRelic.BeforeCombatStart"/>, so it
/// activates for <em>any</em> character that holds a crit contributor —
/// not just Shion.
///
/// Because it is a <see cref="MegaCrit.Sts2.Core.Entities.Powers.PowerStackType.Single"/>
/// power, repeated application is a no-op; exactly one instance exists per
/// creature per combat.
/// 
/// Crit rates are CACHED and recalculated when:
/// - Combat starts
/// - A power is applied/removed on the owner
/// - A relic is obtained (handled by relic's BeforeCombatStart)
/// </summary>
public class ShionCritTrackerPower : ShionTheBruiserPower
{
    public override MegaCrit.Sts2.Core.Entities.Powers.PowerType Type =>
        MegaCrit.Sts2.Core.Entities.Powers.PowerType.None;

    public override MegaCrit.Sts2.Core.Entities.Powers.PowerStackType StackType =>
        MegaCrit.Sts2.Core.Entities.Powers.PowerStackType.Single;

    // ── DynamicVars ───────────────────────────────────────────────────────────

    /// <summary>
    /// Stored/cached crit rates displayed in tooltips.
    /// These are recalculated when combat starts or powers/relics change.
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HitRate", 0m),    // Per-hit critical chance percentage
        new DynamicVar("AttackRate", 0m), // Whole-attack critical chance percentage
    ];

    // ── Recalculation ──────────────────────────────────────────────────────────

    /// <summary>
    /// Recalculates and updates the stored crit rate values.
    /// Called when combat starts, powers change, or relics are obtained.
    /// </summary>
    private void RecalculateCritRates()
    {
        if (Owner.CombatState == null)
        {
            MainFile.Logger.Info("[ShionCritTrackerPower] RecalculateCritRates: No CombatState, skipping");
            return;
        }

        MainFile.Logger.Info($"[ShionCritTrackerPower] RecalculateCritRates for {Owner.GetType().Name}");

        // Calculate baseline rates (what appears in tooltip)
        decimal hitRate = ShionHooksHelper.CalculateCritHitRate(Owner.CombatState, null, Owner.Player);
        decimal attackRate = ShionHooksHelper.CalculateCritAttackRate(Owner.CombatState, null, Owner.Player);

        // Convert to percentages and store
        DynamicVars["HitRate"].BaseValue = Math.Round(hitRate * 100m, 0, MidpointRounding.AwayFromZero);
        DynamicVars["AttackRate"].BaseValue = Math.Round(attackRate * 100m, 0, MidpointRounding.AwayFromZero);

        MainFile.Logger.Info($"[ShionCritTrackerPower] Updated rates: HitRate={DynamicVars["HitRate"].BaseValue}%, AttackRate={DynamicVars["AttackRate"].BaseValue}%");
        
        // Notify UI to update
        InvokeDisplayAmountChanged();
    }

    // ── Recalculation Triggers ────────────────────────────────────────────────

    /// <summary>
    /// Recalculate when combat starts (after all relics/powers are set up).
    /// </summary>
    public override Task BeforeCombatStart()
    {
        MainFile.Logger.Info("[ShionCritTrackerPower] BeforeCombatStart - will recalculate after base call");
        RecalculateCritRates();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Recalculate when any power changes on the owner creature.
    /// </summary>
    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext context,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        // Only recalculate if the power belongs to our owner
        if (power.Owner == Owner)
        {
            MainFile.Logger.Info($"[ShionCritTrackerPower] AfterPowerAmountChanged: {power.GetType().Name} amount={amount}");
            RecalculateCritRates();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Recalculate at the start of each turn (in case something changed).
    /// </summary>
    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner))
        {
            MainFile.Logger.Info("[ShionCritTrackerPower] BeforeSideTurnStart - recalculating");
            RecalculateCritRates();
        }

        return Task.CompletedTask;
    }

    // ── Combat hooks ─────────────────────────────────────────────────────────

    /// <summary>
    /// Fires once before every attack the owning creature makes.
    /// Performs the whole-attack critical roll and resets per-hit state.
    /// </summary>
    public override Task BeforeAttack(AttackCommand command)
    {
        MainFile.Logger.Info($"[ShionCritTrackerPower] BeforeAttack fired. Attacker={command.Attacker?.GetType().Name ?? "null"}, Owner={Owner.GetType().Name}, isMatch={command.Attacker == Owner}, ModelSource={command.ModelSource?.GetType().Name ?? "null"}");

        if (command.Attacker != Owner)
            return Task.CompletedTask;

        var card = command.ModelSource as CardModel;
        MainFile.Logger.Info($"[ShionCritTrackerPower] Calling StartAttack. Card={card?.GetType().Name ?? "null (no card)"}");
        ShionCritSystem.StartAttack(Owner.CombatState!, card);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Applies the 1.5× critical multiplier per damage-calculation call.
    /// Performs the per-hit crit roll on the first call for each hit (idempotent
    /// for additional AOE targets), then returns the multiplier.
    ///
    /// This fires for any attack the owning creature makes, covering both the
    /// character's own cards and any neutral cards they play.
    /// </summary>
    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        MainFile.Logger.Info($"[ShionCritTrackerPower] ModifyDamageMultiplicative fired. dealer={dealer?.GetType().Name ?? "null"}, Owner={Owner.GetType().Name}, isMatch={dealer == Owner}, card={cardSource?.GetType().Name ?? "null"}");

        if (dealer != Owner)
            return 1m;

        ShionCritSystem.EnsureHitRolled(Owner.CombatState!, cardSource);
        decimal multiplier = ShionCritSystem.CurrentHitIsCrit ? 1.5m : 1m;
        MainFile.Logger.Info($"[ShionCritTrackerPower] CurrentHitIsCrit={ShionCritSystem.CurrentHitIsCrit}, returning multiplier={multiplier}");
        return multiplier;
    }

    /// <summary>
    /// Plays critical-hit SFX / spawns gold VFX when a crit connects, then
    /// resets the per-hit roll guard so the next hit gets an independent roll.
    /// </summary>
    public override Task AfterDamageGiven(
        PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner) return Task.CompletedTask;
        if (ShionCritSystem.CurrentHitIsCrit && result.UnblockedDamage > 0)
        {
            // TODO: replace with real FMOD event / VFX scene paths once assets are ready.
            TalkCmd.Play(new LocString("combat_messages", "SHIONTHEBRUISER-CRIT"), dealer, VfxColor.Purple, VfxDuration.VeryShort);
            // SfxCmd.Play("event:/sfx/characters/shion/crit_hit");
            // VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_crit");
        }

        ShionCritSystem.EndHit();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Fully resets crit state after the entire attack sequence finishes.
    /// </summary>
    public override Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker == Owner)
            ShionCritSystem.EndAttack();

        return Task.CompletedTask;
    }
}
