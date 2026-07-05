using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Enums;

public static class DreamersKeywords
{
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword EmeraldPower;
}