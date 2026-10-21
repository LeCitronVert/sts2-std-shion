using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// The first X times a Request card is played each combat, decrease its Request by 1.
/// </summary>
public class SummonBiggerFish : FryTheCommanderPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => Math.Max(0, Amount - _uses);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(FryCardKeywords.Request)];

    private int _uses;

    public bool TryConsume()
    {
        if (DisplayAmount <= 0) return false;

        _uses++;
        Flash();
        InvokeDisplayAmountChanged();
        return true;
    }
}
