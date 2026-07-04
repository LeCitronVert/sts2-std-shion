using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class ExtremeSpeed() : CustomTemporaryPowerModelWrapper<ShionExtremeSpeed, SlowPower>
{
    public override PowerType Type => PowerType.Debuff;
}