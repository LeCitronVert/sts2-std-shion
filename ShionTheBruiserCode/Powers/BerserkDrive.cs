using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class BerserkDrive : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    /// <summary>
    /// Display the current damage bonus percentage based on missing HP.
    /// </summary>
    public override int DisplayAmount
    {
        get
        {
            var missingHpRatio = (Owner.MaxHp - Owner.CurrentHp) / (float)Owner.MaxHp;
            return (int)Math.Round(missingHpRatio * Amount);
        }
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DisplayAmount", 0)
    ];

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

    /// <summary>
    /// Update the display when the owner's HP changes.
    /// </summary>
    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature == Owner)
        {
            InvokeDisplayAmountChanged();
            DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        }
        return Task.CompletedTask;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        InvokeDisplayAmountChanged();
        DynamicVars["DisplayAmount"].BaseValue = DisplayAmount;
        
        return Task.CompletedTask;
    }

}