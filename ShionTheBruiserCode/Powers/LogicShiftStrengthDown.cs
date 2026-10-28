using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class LogicShiftStrengthDown : CustomTemporaryPowerModelWrapper<SlalLogicShift, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
