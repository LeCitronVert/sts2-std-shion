using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Maim X: the enemy loses X Strength this turn.
/// </summary>
public class MaimStrengthDown : CustomTemporaryPowerModelWrapper<SeelsMutilate, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;

    // Titled "Maim" rather than after the origin card (any card can have Maim).
    public override LocString Title => new("powers", Id.Entry + ".title");
}
