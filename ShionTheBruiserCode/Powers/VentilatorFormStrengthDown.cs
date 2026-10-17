using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class VentilatorFormStrengthDown : CustomTemporaryPowerModelWrapper<SlalVentilatorForm, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;
    public override string CustomPackedIconPath => "ventilator_form.png".PowerImagePath();
    public override string CustomBigIconPath => "ventilator_form.png".BigPowerImagePath();
}
