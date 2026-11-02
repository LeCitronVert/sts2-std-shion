using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class JokeAndRunStrengthDown : CustomTemporaryPowerModelWrapper<SeelsJokeAndRun, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
