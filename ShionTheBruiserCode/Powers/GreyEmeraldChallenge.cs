using MegaCrit.Sts2.Core.Entities.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Grey challenge (on each player): at the end of your turn, Exhaust 1 random card in your Discard Pile per stack.
/// </summary>
public class GreyEmeraldChallenge : BlueEmeraldChallenge
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Grey;

    protected override PileType PileToExhaustFrom => PileType.Discard;
}
