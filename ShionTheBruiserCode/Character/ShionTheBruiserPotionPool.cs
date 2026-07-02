using BaseLib.Abstracts;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using Godot;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

public class ShionTheBruiserPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => ShionTheBruiser.Color;


    public override string BigEnergyIconPath => "charui/shion_big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/shion_text_energy.png".ImagePath();
}