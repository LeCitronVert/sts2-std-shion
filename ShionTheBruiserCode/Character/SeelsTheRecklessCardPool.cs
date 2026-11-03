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
    // its chroma (S) and brightness (V). At this hue: out V ≈ 0.378·V, out chroma ≈ 0.609·S·V
    // (fitted from #f1c232 = S 0.492/V 2.499 and #c8a85a = S 0.341/V 2.073).
    // #f1c232 was too bright, #c8a85a too gray; now a deep, saturated gold (#d4ab17).
    public override float H => 0.131f;
    public override float S => 0.554f;
    public override float V => 2.198f;

    //Color of small card icons
    public override Color DeckEntryCardColor => new("d4ab17");

    public override bool IsColorless => false;

    // Content of unrevealed epochs is locked (Timeline/EpochLocks.cs)
    protected override IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards) =>
        EpochLocks.Filter(unlockState, cards);
}
