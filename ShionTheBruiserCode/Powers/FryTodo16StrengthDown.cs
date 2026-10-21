using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class FryTodo16StrengthDown : CustomTemporaryPowerModelWrapper<FryTodo16, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
