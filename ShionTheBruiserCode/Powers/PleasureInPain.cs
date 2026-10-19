using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Variables;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

// Amount = how much the Berserk and Patch Up thresholds are raised, in % of Max HP
// (see ShionMechanicsHelper.GetBerserkThreshold / GetPatchUpThreshold)
public class PleasureInPain() : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public const int DamageTakenIncrease = 30;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DamageTakenIncrease", DamageTakenIncrease),
        new PatchUpVar(0)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];

    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return target != this.Owner || !props.IsPoweredAttack() ? 1M : 1 + (DamageTakenIncrease / 100M);
    }
}
