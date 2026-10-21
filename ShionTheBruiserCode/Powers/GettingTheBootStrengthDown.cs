using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class GettingTheBootStrengthDown : CustomTemporaryPowerModelWrapper<FryGettingTheBoot, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
