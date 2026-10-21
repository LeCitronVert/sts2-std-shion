using BaseLib.Abstracts;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using Godot;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

public class SeelsTheRecklessPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => SeelsTheReckless.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}
