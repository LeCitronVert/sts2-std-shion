using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Critical;

/// <summary>
/// Abstract base class for any relic that contributes to the Critical Damage
/// system (by implementing one or more of <see cref="ICritHitRateContributor"/>,
/// <see cref="ICritAttackRateContributor"/>, <see cref="IMinCritHitRateContributor"/>,
/// or <see cref="IMinCritAttackRateContributor"/>).
///
/// Automatically applies <see cref="Powers.ShionCritTrackerPower"/> to the
/// relic owner's creature at the start of every combat. This means the crit
/// mechanic activates for <em>any</em> character who holds this relic —
/// not just Shion.
///
/// This class intentionally does NOT carry a <c>[Pool]</c> attribute. Subclasses
/// should add <c>[Pool(typeof(...))]</c> themselves to control which character's
/// relic pool they appear in. Shion-specific crit relics should additionally
/// extend or annotate with <c>[Pool(typeof(ShionTheBruiserRelicPool))]</c>.
///
/// Image paths must be provided by each concrete subclass via the inherited
/// <see cref="CustomRelicModel"/> image-path properties.
/// </summary>
public abstract class CritContributorRelic : CustomRelicModel
{
    /// <summary>
    /// Ensures the <see cref="ShionCritTrackerPower"/> is on the owner's
    /// creature before any turn begins. Uses <c>StackType.Single</c> semantics
    /// so that multiple contributors sharing an owner only apply one instance.
    /// </summary>
    public override async Task BeforeCombatStart()
    {
        if (Owner?.Creature != null && !Owner.Creature.HasPower<ShionCritTrackerPower>())
        {
            await PowerCmd.Apply<ShionCritTrackerPower>(
                new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                amount: 1m,
                applier: Owner.Creature,
                cardSource: null);
        }
    }
}

