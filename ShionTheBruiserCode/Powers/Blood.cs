using MegaCrit.Sts2.Core.Entities.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Seels' resource. Gained when losing HP (Thorn Bracelet), consumed by Blood Bend.
/// </summary>
public class Blood : SeelsTheRecklessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
