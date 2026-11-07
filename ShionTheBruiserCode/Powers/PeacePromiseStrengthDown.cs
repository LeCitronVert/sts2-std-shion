using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>Peace Promise's "lose Strength this turn". Uses BaseLib's temporary-down icon.</summary>
public class PeacePromiseStrengthDown : CustomTemporaryPowerModelWrapper<PeacePromise, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
}
