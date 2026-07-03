using BaseLib.Abstracts;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class AnOpening : CustomTemporaryPowerModelWrapper<ShionAnOpening, CritRateUp>
{
    protected override int LastForXExtraTurns => 2;
}