using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Enums;

public class SlalCardKeywords
{
    [CustomEnum, KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Novel;
    
    [CustomEnum, KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Slimy;

    /// <summary>Fills your Hand with Slimed. Counts as Slimy.</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword VerySlimy;
}