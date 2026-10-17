using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Fresh Ideas transform ALL cards in your Hand and grant X additional Random.
/// Random cards you create can be Colorless.
/// Effects are handled in <see cref="SlalFreshIdea"/> and <see cref="Helper.SlalMechanicsHelper.GetRandomCardOptions"/>.
/// </summary>
public class TelevisionForm : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalFreshIdea>(),
        HoverTipFactory.Static(StaticHoverTip.Transform),
        HoverTipFactory.FromPower<Random>(),
    ];
}
