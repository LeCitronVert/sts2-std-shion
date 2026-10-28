using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Enums;

public static class DreamersKeywords
{
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword EmeraldPower;
    
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword TeamBlast;

    // Its "Team-Up: (Class)" line is added by TeamUpDescriptionPatch, since it names a class.
    [CustomEnum, KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword TeamUp;
}