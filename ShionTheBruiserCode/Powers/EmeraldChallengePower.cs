using MegaCrit.Sts2.Core.Entities.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Base class for the Special Stage Elite challenges. Amount = number of Special Stage quests of that color in the fight.
/// </summary>
public abstract class EmeraldChallengePower : ShionTheBruiserPower
{
    public abstract ChaosEmeraldColor Color { get; }

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private string EmeraldImage => $"emeralds/emerald_solo_{ChaosEmeraldHelper.ColorKey(Color)}.png".RelicImagePath();

    public override string CustomPackedIconPath => EmeraldImage;
    public override string CustomBigIconPath => EmeraldImage;
}
