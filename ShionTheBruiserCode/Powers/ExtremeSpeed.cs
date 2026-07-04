using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class ExtremeSpeed() : CustomTemporaryPowerModelWrapper<ShionExtremeSpeed, SlowPower>
{
    public override PowerType Type => PowerType.Debuff;
    
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}