using BaseLib.Abstracts;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using Godot;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

public class SeelsTheRecklessCardPool : CustomCardPoolModel
{
    public override string Title => SeelsTheReckless.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // HSV of #add8e6 (light blue).
    public override float H => 0.542f;
    public override float S => 0.248f;
    public override float V => 0.902f;

    //Color of small card icons
    public override Color DeckEntryCardColor => new("add8e6");

    public override bool IsColorless => false;
}
