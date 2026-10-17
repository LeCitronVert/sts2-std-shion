using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// While Weak, deal 25% (per stack) more damage to Weak enemies, and 25% (per stack) more damage to Confused enemies.
/// Both bonuses are additive.
/// </summary>
public class PirateParrotForm : SlalTheEndlessPower
{
    public const decimal BonusPerStack = 0.25m;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<Confusion>(),
    ];

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer != Owner || target == null || !props.IsPoweredAttack() || !Owner.HasPower<WeakPower>())
            return 1m;

        var bonus = 0m;
        if (target.HasPower<WeakPower>()) bonus += BonusPerStack * Amount;
        if (target.HasPower<Confusion>()) bonus += BonusPerStack * Amount;
        return 1m + bonus;
    }
}
