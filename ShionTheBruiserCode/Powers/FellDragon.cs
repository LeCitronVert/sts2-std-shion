using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

// Always apply Galvanised and Berserk effects (see ShionMechanicsHelper.IsGalvanised / IsBerserk),
// Patch Up limit reduced (see ShionMechanicsHelper.GetPatchUpThreshold)
public class FellDragon : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;

    public const int PatchUpReduction = 20;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("PatchUpReduction", PatchUpReduction),
        new PatchUpVar(0)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];
}
