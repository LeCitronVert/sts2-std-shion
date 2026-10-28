using MegaCrit.Sts2.Core.Entities.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// This combat, random cards you create can be Colorless (see <see cref="Helper.SlalMechanicsHelper.GetRandomCardOptions"/>).
/// </summary>
public class MoodShift : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}
