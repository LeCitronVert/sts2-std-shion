using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class OctopusFormStrengthDown : CustomTemporaryPowerModelWrapper<SlalOctopusForm, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
    public override string CustomPackedIconPath => "octopus_form.png".PowerImagePath();
    public override string CustomBigIconPath => "octopus_form.png".BigPowerImagePath();
}
