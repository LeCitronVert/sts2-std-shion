using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Critical;

/// <summary>
/// Abstract base class for any power that contributes to the Critical Damage
/// system (by implementing one or more of <see cref="ICritHitRateContributor"/>,
/// <see cref="ICritAttackRateContributor"/>, <see cref="IMinCritHitRateContributor"/>,
/// or <see cref="IMinCritAttackRateContributor"/>).
///
/// Automatically applies <see cref="Powers.ShionCritTrackerPower"/> to the
/// power's owner at the start of every combat. This means the crit mechanic
/// activates for <em>any</em> character who holds this power — not just Shion.
///
/// Image paths follow <c>ShionTheBruiser/images/powers/power_name.png</c>
/// via the inherited <see cref="ShionTheBruiserPower"/> convention.
/// </summary>
public abstract class CritContributorPower : ShionTheBruiserPower
{
    /// <summary>
    /// Ensures the <see cref="ShionCritTrackerPower"/> is on the owner's
    /// creature before any turn begins. Uses <c>StackType.Single</c> semantics
    /// so that multiple contributors sharing an owner only apply one instance.
    /// </summary>
    public override async Task BeforeCombatStart()
    {
        if (!Owner.HasPower<ShionCritTrackerPower>())
        {
            await PowerCmd.Apply<ShionCritTrackerPower>(
                new ThrowingPlayerChoiceContext(),
                Owner,
                amount: 1m,
                applier: Owner,
                cardSource: null);
        }
    }

    /// <summary>
    /// Applied mid-combat (card, potion, other power): BeforeCombatStart has already run, so add the tracker now,
    /// or the crit indicator never shows. Recalculated here since this power was applied before the tracker existed.
    /// </summary>
    public override async Task AfterApplied(Creature? applier, MegaCrit.Sts2.Core.Models.CardModel? cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        if (Owner.CombatState == null) return;
        await ShionMechanicsHelper.EnsureCreatureHasCritTracker(Owner);
        Owner.GetPower<ShionCritTrackerPower>()?.RecalculateCritRates();
    }

    /// <summary>
    /// PowerCmd.Remove fires no AfterPowerAmountChanged, so refresh the cached crit
    /// display once this contributor is actually gone from the owner.
    /// </summary>
    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        oldOwner.GetPower<ShionCritTrackerPower>()?.RecalculateCritRates();
    }
}
