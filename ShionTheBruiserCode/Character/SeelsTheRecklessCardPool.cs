using BaseLib.Abstracts;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using ShionTheBruiser.ShionTheBruiserCode.Timeline;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

public class SeelsTheRecklessCardPool : CustomCardPoolModel
{
    public override string Title => SeelsTheReckless.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // Shader params, NOT absolute HSV: hsv.gdshader hue-rotates the red base frame and multiplies
    // its chroma (S) and brightness (V). Was #f1c232 (H 0.131, S 0.492, V 2.499), too bright;
    // scaled down from those values to a softer, muted gold (~#c8a85a).
    public override float H => 0.131f;
    public override float S => 0.341f;
    public override float V => 2.073f;

    //Color of small card icons
    public override Color DeckEntryCardColor => new("c8a85a");

    public override bool IsColorless => false;

    // Content of unrevealed epochs is locked (Timeline/EpochLocks.cs)
    protected override IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards) =>
        EpochLocks.Filter(unlockState, cards);
}
