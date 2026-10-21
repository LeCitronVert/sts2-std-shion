using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class PunchParryStrengthDown : CustomTemporaryPowerModelWrapper<FryPunchParry, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
