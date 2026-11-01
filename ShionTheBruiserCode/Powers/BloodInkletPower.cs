using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// On a Blood Inklet (Royal Mage): takes the unblocked attack damage meant for its owner, like Osty.
/// It also takes the hits meant for its owner's Osty, so it always takes them first, whatever order the hooks run in.
/// </summary>
public class BloodInkletPower : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        if (Owner.IsDead || !props.IsPoweredAttack()) return target;
        if (Owner.PetOwner is not { } owner) return target;

        return target == owner.Creature || target == owner.Osty ? Owner : target;
    }

    // It can't be targeted with powers once dead.
    public override bool ShouldAllowHitting(Creature creature)
    {
        return creature != Owner || creature.IsAlive;
    }
}
