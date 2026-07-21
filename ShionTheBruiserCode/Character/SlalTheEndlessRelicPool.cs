using BaseLib.Abstracts;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using Godot;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

public class SlalTheEndlessRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => SlalTheEndless.Color;

    public override string BigEnergyIconPath => "charui/slal_big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/slal_text_energy.png".ImagePath();
}

