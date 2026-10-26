using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class GreenChallengeDexterityDown : CustomTemporaryPowerModelWrapper<GreenEmeraldChallenge, DexterityPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
