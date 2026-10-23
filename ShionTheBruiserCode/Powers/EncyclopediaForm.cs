using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Memory no longer plays cards: each Memory gives 1 energy and 1 card draw next turn instead.
/// Handled in <see cref="Helper.SlalMechanicsHelper.PlayFromMemory"/>.
/// </summary>
public class EncyclopediaForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Memory>(),
    ];
}
