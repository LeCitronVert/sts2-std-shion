using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Variables;

/// <summary>
/// A lazily-evaluated <see cref="DynamicVar"/> that reads the current effective critical
/// hit or critical attack rate from <see cref="ShionHooksHelper"/> at the moment the
/// hover tip text is rendered.
///
/// Because <see cref="GetBaseValueForIConvertible"/> is the method SmartFormat calls when
/// substituting <c>{HitRate}</c> / <c>{AttackRate}</c> tokens into the localization string,
/// the displayed percentage always reflects:
/// <list type="bullet">
///   <item>the sum of all active <see cref="Critical.ICritHitRateContributor"/> bonuses, and</item>
///   <item>clamped upward by the highest <see cref="Critical.IMinCritHitRateContributor"/> floor.</item>
/// </list>
///
/// Returns 0 for canonical (non-mutable) power instances, since there is no combat
/// context available outside of combat.
///
/// Note: contributors that filter on a specific <c>sourceCard</c> type (e.g.
/// <c>sourceCard is ShionTheBruiserCard</c>) will return 0 when this var queries them with
/// <c>null</c>. The displayed rate therefore represents the baseline rate that applies to
/// unfiltered attacks (relics and unconditional powers). This is the most useful
/// summary for the hover tip.
/// </summary>
public class CritRateDisplayVar : DynamicVar
{
    public const string HitRateKey = "HitRate";
    public const string AttackRateKey = "AttackRate";

    private readonly bool _isAttackRate;

    /// <param name="isAttackRate">
    /// Pass <c>true</c> to track the whole-attack critical chance;
    /// pass <c>false</c> to track the per-hit critical chance.
    /// </param>
    public CritRateDisplayVar(bool isAttackRate)
        : base(isAttackRate ? AttackRateKey : HitRateKey, 0m)
    {
        _isAttackRate = isAttackRate;
    }

    /// <summary>
    /// Returns the current effective crit rate as a rounded integer percentage (0–100+),
    /// computed lazily at render time from the live combat state.
    /// </summary>
    protected override decimal GetBaseValueForIConvertible()
    {
        // Only works for mutable (in-combat) power instances.
        if (_owner is not PowerModel power || !power.IsMutable)
            return 0m;

        var cs = power.Owner.CombatState;
        if (cs == null) return 0m;

        decimal rate = _isAttackRate
            ? ShionHooksHelper.CalculateCritAttackRate(cs, null, power.Owner.Player)
            : ShionHooksHelper.CalculateCritHitRate(cs, null, power.Owner.Player);

        return Math.Round(rate * 100m, 0, MidpointRounding.AwayFromZero);
    }
}


