using BaseLib.Abstracts;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using ShionTheBruiser.ShionTheBruiserCode.Timeline;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

public class FryTheCommanderCardPool : CustomCardPoolModel
{
    public override string Title => FryTheCommander.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // HSV of #cc0000 (red).
    public override float H => 0f;
    public override float S => 1f;
    public override float V => 0.8f;

    //Color of small card icons
    public override Color DeckEntryCardColor => new("cc0000");

    public override bool IsColorless => false;

    // Content of unrevealed epochs is locked (Timeline/EpochLocks.cs)
    protected override IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards) =>
        EpochLocks.Filter(unlockState, cards);
}
