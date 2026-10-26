using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class GreenChallengeStrengthDown : CustomTemporaryPowerModelWrapper<GreenEmeraldChallenge, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
