using MegaCrit.Sts2.Core.Entities.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Absorption: you can no longer gain Max HP this combat.
/// Handled in <see cref="Patches.MaxHpLockedPatch"/>. Powers are removed at the end of combat, so it doesn't last longer.
/// </summary>
public class MaxHpLocked : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
}
