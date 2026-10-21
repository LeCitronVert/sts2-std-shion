// Powers/ShionCritTrackerPower.cs
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Display-only power now: tracks the cached crit rate of the owning creature.
/// It is hidden from the power bar: the rate is shown by the crit indicator above
/// the energy counter instead (see <see cref="Visuals.CritIndicatorPatch"/>). All actual crit rolling, the attack stack, the
/// damage multiplier, and the OnCriticalHit/OnCriticalFail dispatch have
/// moved to <see cref="Critical.ShionCritSingleton"/>, which gates its
/// global combat hooks on whether the attacker carries this power.
///
/// Applied automatically to any creature whose combat-active models
/// include a crit contributor, via CritContributorPower.BeforeCombatStart
/// or CritContributorRelic.BeforeCombatStart (unchanged).
/// </summary>
public class ShionCritTrackerPower : ShionTheBruiserPower
{
    public override MegaCrit.Sts2.Core.Entities.Powers.PowerType Type =>
        MegaCrit.Sts2.Core.Entities.Powers.PowerType.None;

    public override MegaCrit.Sts2.Core.Entities.Powers.PowerStackType StackType =>
        MegaCrit.Sts2.Core.Entities.Powers.PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HitRate", 0m),
        new DynamicVar("AttackRate", 0m),
    ];

    protected override bool IsVisibleInternal => false;

    /// <summary>Current crit chance per hit, in percent.</summary>
    public int HitRatePercent => (int) DynamicVars["HitRate"].BaseValue;

    /// <summary>
    /// The tooltip this power would show if it was visible (<see cref="PowerModel.HoverTips"/> is empty while hidden).
    /// </summary>
    public HoverTip CreateIndicatorHoverTip()
    {
        var description = SmartDescription;
        DynamicVars.AddTo(description);
        return new HoverTip(this, description.GetFormattedText(), true);
    }

    private void RecalculateCritRates()
    {
        if (Owner.CombatState == null)
        {
            MainFile.Logger.Info("[ShionCritTrackerPower] RecalculateCritRates: No CombatState, skipping");
            return;
        }

        decimal hitRate = ShionHooksHelper.CalculateCritHitRate(Owner.CombatState, null, Owner.Player);
        decimal attackRate = ShionHooksHelper.CalculateCritAttackRate(Owner.CombatState, null, Owner.Player);

        DynamicVars["HitRate"].BaseValue = Math.Round(hitRate * 100m, 0, MidpointRounding.AwayFromZero);
        DynamicVars["AttackRate"].BaseValue = Math.Round(attackRate * 100m, 0, MidpointRounding.AwayFromZero);

        MainFile.Logger.Info($"[ShionCritTrackerPower] Updated rates: HitRate={DynamicVars["HitRate"].BaseValue}%, AttackRate={DynamicVars["AttackRate"].BaseValue}%");

        InvokeDisplayAmountChanged();
    }

    public override Task BeforeCombatStart()
    {
        RecalculateCritRates();
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        RecalculateCritRates();
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext context, PowerModel power, decimal amount, MegaCrit.Sts2.Core.Entities.Creatures.Creature? applier, CardModel? cardSource)
    {
        if (power.Owner == Owner)
        {
            MainFile.Logger.Info($"[ShionCritTrackerPower] AfterPowerAmountChanged: {power.GetType().Name} amount={amount}");
            RecalculateCritRates();
        }

        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<MegaCrit.Sts2.Core.Entities.Creatures.Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner))
        {
            RecalculateCritRates();
        }

        return Task.CompletedTask;
    }
}