using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class HammerArm : CustomTemporaryPowerModelWrapper<ShionHammerArm, StrengthPower>
{
    public const int Duration = 3;

    protected override bool InvertInternalPowerAmount => true;
    public override PowerType Type => PowerType.Debuff;
    protected override int LastForXExtraTurns => Duration;
}