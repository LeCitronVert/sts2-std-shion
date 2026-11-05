using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Maim X: the enemy loses X Strength this turn.
/// </summary>
public class MaimStrengthDown : CustomTemporaryPowerModelWrapper<SeelsMutilate, StrengthPower>
{
    protected override bool InvertInternalPowerAmount => true;

    // Titled "Maim" rather than after the origin card (any card can have Maim).
    public override LocString Title => new("powers", Id.Entry + ".title");

    // Show the Maim keyword instead of the origin card (Mutilate is only the wrapper's required source type).
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Maim),
        HoverTipFactory.FromPower(InternallyAppliedPower),
    ];
}
