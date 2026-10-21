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
}
