using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// At the start of each combat, add its Emerald Power to your Draw Pile (see <see cref="ChaosEmeraldRelic"/>).
/// </summary>
public class RedChaosEmerald() : ChaosEmeraldRelic
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Red;
}
