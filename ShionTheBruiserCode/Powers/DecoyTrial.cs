using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you gain Bad Faith, Refuse or lose Damage Debt from a card, ALL other players benefit from it too.
/// Handled in <see cref="Helper.FryMechanicsHelper"/>. Does not stack.
/// </summary>
public class DecoyTrial : FryTheCommanderPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<BadFaith>(),
        HoverTipFactory.FromPower<Refuse>(),
        HoverTipFactory.FromPower<DamageDebt>(),
    ];
}
