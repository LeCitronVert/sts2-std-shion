using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Enums;

public static class FryCardKeywords
{
    // Hover tip only keywords: their amount is written in the card text.
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Cooldown;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Request;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Refuse;

    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Valkyrs;
}
