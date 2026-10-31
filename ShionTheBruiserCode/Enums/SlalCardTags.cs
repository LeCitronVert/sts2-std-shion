namespace ShionTheBruiser.ShionTheBruiserCode.Enums;

using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

public static class SlalCardTags
{
    [CustomEnum]
    public static CardTag Form;

    /// <summary>Cards whose effect Shapeshifts (Free Shapeshift included). Hat Tip triggers on them.</summary>
    [CustomEnum]
    public static CardTag Shapeshift;

    /// <summary>Cards whose effect Mimics. Hat Tip triggers on them.</summary>
    [CustomEnum]
    public static CardTag Mimic;
}