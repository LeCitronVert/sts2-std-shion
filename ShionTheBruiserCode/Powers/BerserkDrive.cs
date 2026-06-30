using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class BerserkDrive() : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (null == dealer) return 1M;
        
        return dealer != this.Owner && !this.Owner.Pets.Contains<Creature>(dealer) || !props.IsPoweredAttack() || cardSource == null ? 1M : GetBerserkDriveMultiplier(Owner);
    }
    
    private decimal GetBerserkDriveMultiplier(Creature creature)
    {
        // Amount = percentage of damage but a 0 - 1 decimal is expected, based on the max hp and missing ratio
        var missingHpRatio = (creature.MaxHp - creature.CurrentHp)
                             / (float) creature.MaxHp;

        return ((decimal) (missingHpRatio * (float) (Amount * 0.01M))) + 1;
    }
}