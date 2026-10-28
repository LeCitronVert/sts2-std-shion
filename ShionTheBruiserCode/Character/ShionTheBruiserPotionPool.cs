using BaseLib.Abstracts;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using ShionTheBruiser.ShionTheBruiserCode.Timeline;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

public class ShionTheBruiserPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => ShionTheBruiser.Color;


    public override string BigEnergyIconPath => "charui/shion_big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/shion_text_energy.png".ImagePath();

    // Content of unrevealed epochs is locked (Timeline/EpochLocks.cs)
    public override IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState) =>
        EpochLocks.Filter(unlockState, base.GetUnlockedPotions(unlockState));
}