using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Enums;

public static class SeelsCardKeywords
{
    /// <summary>Add a Wound to your Hand when played (before the card's effect).</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Wounding;

    /// <summary>When drawn, the card's cost is randomized. Removes Unplayable.</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Erratic;

    // Hover tip only keywords: their amount is written in the card text.
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword BloodBend;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Gamble;

    /// <summary>
    /// Maim X: if an enemy loses HP from this card, it loses X Strength this turn and you gain X Blood (per hit).
    /// Hover tip only: the "Maim X." line is written by <see cref="Patches.MaimDescriptionPatch"/>.
    /// </summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Maim;

    /// <summary>Bloody Daggers: the Shiv deals X additional damage, then its owner Blood Bends.</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword BloodyShiv;
}
