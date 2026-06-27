using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class DragonRoar : CustomTemporaryPowerModelWrapper<ShionDragonRoar, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
    public override PowerType Type => PowerType.Debuff;
}
