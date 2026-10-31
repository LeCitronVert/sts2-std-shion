using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Enums;

public class SlalCardKeywords
{
    [CustomEnum, KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Novel;

    /// <summary>Novel that isn't removed after being played.</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword AlwaysNovel;
    
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Slimy;

    /// <summary>Fills your Hand with Slimed. Counts as Slimy.</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword VerySlimy;

    // Hover tip only keywords: written in the card text.

    /// <summary>Add a random Goofball Token Power into your Hand.</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Shapeshift;

    /// <summary>Shapeshift, and the Goofball Token Power is free to play this turn.</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword FreeShapeshift;

    /// <summary>Choose 1 of X random non-Rare cards from other characters to add into your Hand.</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Mimic;

    /// <summary>Lists the Goofball Token Powers.</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword GoofballTokenPower;
}
