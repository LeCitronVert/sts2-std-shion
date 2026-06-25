using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

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
/// </summary>
public class ShionCritTrackerPower : ShionTheBruiserPower
{
    public override MegaCrit.Sts2.Core.Entities.Powers.PowerType Type =>
        MegaCrit.Sts2.Core.Entities.Powers.PowerType.Buff;

    public override MegaCrit.Sts2.Core.Entities.Powers.PowerStackType StackType =>
        MegaCrit.Sts2.Core.Entities.Powers.PowerStackType.Single;

    // ── DynamicVars ───────────────────────────────────────────────────────────

    /// <summary>
    /// Exposes the live crit rates as lazily-computed tokens for the smart description.
    /// <c>{HitRate}</c>    → per-hit critical chance as a whole integer percentage.
    /// <c>{AttackRate}</c> → whole-attack critical chance as a whole integer percentage.
    /// Both include the minimum-rate floor and are computed fresh on every hover-tip render.
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CritRateDisplayVar(isAttackRate: false),  // {HitRate}
        new CritRateDisplayVar(isAttackRate: true),   // {AttackRate}
    ];

    // ── Combat hooks ─────────────────────────────────────────────────────────

    /// <summary>
    /// Fires once before every attack the owning creature makes.
    /// Performs the whole-attack critical roll and resets per-hit state.
    /// </summary>
    public override Task BeforeAttack(AttackCommand command)
    {
        MainFile.Logger.Info($"[ShionCritTrackerPower] BeforeAttack fired. Attacker={command.Attacker?.GetType().Name ?? "null"}, Owner={Owner?.GetType().Name ?? "null"}, isMatch={command.Attacker == Owner}, ModelSource={command.ModelSource?.GetType().Name ?? "null"}");

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
        MainFile.Logger.Info($"[ShionCritTrackerPower] ModifyDamageMultiplicative fired. dealer={dealer?.GetType().Name ?? "null"}, Owner={Owner?.GetType().Name ?? "null"}, isMatch={dealer == Owner}, card={cardSource?.GetType().Name ?? "null"}");

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
