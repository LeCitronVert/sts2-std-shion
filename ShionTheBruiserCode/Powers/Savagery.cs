using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// You deal 10% more damage to Vulnerable enemies for each of your first X Vulnerable.
/// </summary>
public class Savagery : SeelsTheRecklessPower
{
    public const decimal BonusPerVulnerable = 0.1m;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer != Owner || target is null || !props.IsPoweredAttack() || !target.HasPower<VulnerablePower>()) return 1m;

        var stacks = Math.Min(Owner.GetPowerAmount<VulnerablePower>(), Amount);
        return 1m + stacks * BonusPerVulnerable;
    }
}
