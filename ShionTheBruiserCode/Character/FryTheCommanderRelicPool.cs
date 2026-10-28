using BaseLib.Abstracts;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using ShionTheBruiser.ShionTheBruiserCode.Timeline;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

public class FryTheCommanderRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => FryTheCommander.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // Content of unrevealed epochs is locked (Timeline/EpochLocks.cs)
    public override IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState) =>
        EpochLocks.Filter(unlockState, base.GetUnlockedRelics(unlockState));
}
