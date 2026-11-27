using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Fry's status: the Valkyrs deal X more damage and grant X more Block (each instance), for the rest of the combat.
/// Applied in <see cref="Helper.FryMechanicsHelper.ValkyrsDamage"/> / <see cref="Helper.FryMechanicsHelper.ValkyrsBlock"/>.
/// </summary>
public class Command : FryTheCommanderPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(FryCardKeywords.Valkyrs)];
}
